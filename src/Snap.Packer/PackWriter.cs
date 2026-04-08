namespace Snap.Packer;

public sealed class PackWriter : IDisposable
{
    private const uint MAGIC = 0x50595243; // "CRYP" in little-endian
    private const ushort VERSION = 1;
    private const int HEADER_SIZE = 24;

    private readonly List<PackEntry> _entries = new();
    private readonly Dictionary<string, PackEntry> _entryMap = new();
    private readonly List<byte[]> _fileData = new();
    private readonly List<string> _filePaths = new();
    private PackOptions _options;

    public void AddFile(string virtualPath, byte[] data)
    {
        if (string.IsNullOrWhiteSpace(virtualPath))
            throw new ArgumentException("Virtual path cannot be empty", nameof(virtualPath));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        virtualPath = NormalizePath(virtualPath);

        _filePaths.Add(virtualPath);
        _fileData.Add(data);
    }

    public void AddDirectory(string sourcePath, string virtualRoot)
    {
        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        virtualRoot = NormalizePath(virtualRoot);

        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string relativePath = Path.GetRelativePath(sourcePath, file);
            string virtualPath = Path.Combine(virtualRoot, relativePath);
            virtualPath = NormalizePath(virtualPath);

            byte[] data = File.ReadAllBytes(file);
            AddFile(virtualPath, data);
        }
    }

    public void Write(string outputPath, PackOptions options = null)
    {
        _options = options ?? new PackOptions();
        _entries.Clear();
        _entryMap.Clear();

        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // Process each file: compress if beneficial
        List<ProcessedFile> processedFiles = [];

        foreach (var (path, data) in _filePaths.Zip(_fileData, (p, d) => (p, d)))
        {
            var processed = ProcessFile(data, path);
            processedFiles.Add(processed);
        }

        // Calculate offsets for file header (where things are in the final file)
        ulong currentOffset = (ulong)HEADER_SIZE;

        if (_options.Encrypt)
        {
            currentOffset += 16 + 12 + 16; // Salt + IV + AuthTag
        }

        ulong tableOffset = currentOffset;
        using var memoryStream = new MemoryStream();
        using var tableWriter = new BinaryWriter(memoryStream);

        tableWriter.Write((ushort)processedFiles.Count);

        foreach (var file in processedFiles)
        {
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(file.VirtualPath);
            if (pathBytes.Length > 255)
                throw new InvalidOperationException($"Path too long: {file.VirtualPath}");

            tableWriter.Write((byte)pathBytes.Length);
            tableWriter.Write(pathBytes);
            tableWriter.Write(file.Offset); // This will be 0 for now, will fix after rebuild
            tableWriter.Write(file.CompressedSize);
            tableWriter.Write(file.UncompressedSize);
            tableWriter.Write(file.Crc32);
            tableWriter.Write((ushort)file.Flags);
        }

        byte[] fileTable = memoryStream.ToArray();

        // First pass: Set temporary offsets using current file table size
        ulong payloadOffset = (ulong)fileTable.Length;

        for (int i = 0; i < processedFiles.Count; i++)
        {
            var file = processedFiles[i];
            file.Offset = payloadOffset;
            processedFiles[i] = file;
            payloadOffset += file.CompressedSize;
        }

        // Rebuild file table with these temporary offsets
        memoryStream.SetLength(0);
        tableWriter.BaseStream.Position = 0;
        tableWriter.Write((ushort)processedFiles.Count);

        foreach (var file in processedFiles)
        {
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(file.VirtualPath);
            tableWriter.Write((byte)pathBytes.Length);
            tableWriter.Write(pathBytes);
            tableWriter.Write(file.Offset);
            tableWriter.Write(file.CompressedSize);
            tableWriter.Write(file.UncompressedSize);
            tableWriter.Write(file.Crc32);
            tableWriter.Write((ushort)file.Flags);
        }

        fileTable = memoryStream.ToArray();

        // Second pass: Recalculate offsets using the FINAL file table size
        ulong finalPayloadOffset;

        if (_options.Encrypt)
        {
            finalPayloadOffset = (ulong)fileTable.Length;
        }
        else
        {
            finalPayloadOffset = HEADER_SIZE + (ulong)fileTable.Length;
        }

        for (int i = 0; i < processedFiles.Count; i++)
        {
            var file = processedFiles[i];
            file.Offset = finalPayloadOffset;
            processedFiles[i] = file;
            finalPayloadOffset += file.CompressedSize;
        }

        // Rebuild file table ONE MORE TIME with final offsets
        memoryStream.SetLength(0);
        tableWriter.BaseStream.Position = 0;
        tableWriter.Write((ushort)processedFiles.Count);

        foreach (var file in processedFiles)
        {
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(file.VirtualPath);
            tableWriter.Write((byte)pathBytes.Length);
            tableWriter.Write(pathBytes);
            tableWriter.Write(file.Offset);
            tableWriter.Write(file.CompressedSize);
            tableWriter.Write(file.UncompressedSize);
            tableWriter.Write(file.Crc32);
            tableWriter.Write((ushort)file.Flags);
        }

        fileTable = memoryStream.ToArray();

        // Build header in memory first to calculate CRC
        ushort flags = 0;
        if (_options.Encrypt) flags |= (ushort)PackFlags.Encrypted;
        if (_options.CompressionLevel != CompressionLevel.NoCompression) flags |= (ushort)PackFlags.Compressed;
        if (_options.EmbedManifest) flags |= (ushort)PackFlags.HasManifest;

        // Build header without CRC for calculation
        using var headerStream = new MemoryStream();
        using var headerWriter = new BinaryWriter(headerStream);
        headerWriter.Write(MAGIC);
        headerWriter.Write(VERSION);
        headerWriter.Write(flags);
        headerWriter.Write((uint)processedFiles.Count);
        headerWriter.Write(tableOffset);

        byte[] headerBytes = headerStream.ToArray(); // This is 20 bytes
        uint headerCrc = CalculateCrc32(headerBytes);

        // Write final header with CRC
        writer.Write(MAGIC);
        writer.Write(VERSION);
        writer.Write(flags);
        writer.Write((uint)processedFiles.Count);
        writer.Write(tableOffset);
        writer.Write(headerCrc);

        using var payloadStream = new MemoryStream();

        payloadStream.Write(fileTable);
        foreach (var file in processedFiles)
        {
            payloadStream.Write(file.Data);
        }

        if (_options.EmbedManifest)
        {
            var manifest = new PackManifest
            {
                Version = VERSION.ToString(),
                CreatedAt = DateTime.Now,
                FileCount = processedFiles.Count,
                Files = processedFiles.Select(f => new PackManifestEntry
                {
                    VirtualPath = f.VirtualPath,
                    UncompressedSize = f.UncompressedSize,
                    CompressedSize = f.CompressedSize,
                    Crc32 = f.Crc32,
                    Compressed = f.IsCompressed
                }).ToList()
            };

            var json = new JsonSerializerOptions { WriteIndented = true };
            var manifestJson = JsonSerializer.Serialize(manifest, json);
            var manifestData = Encoding.UTF8.GetBytes(manifestJson);

            payloadStream.Write(BitConverter.GetBytes(manifestData.Length));
            payloadStream.Write(manifestData);
        }

        byte[] payload = payloadStream.ToArray();

        // Prepare data for encryption if needed
        byte[] dataToWrite;
        byte[] salt = null;
        byte[] iv = null;
        byte[] authTag = null;

        if (_options.Encrypt && _options.Passkey != null && _options.Passkey.Length > 0)
        {
            // Generate random salt and IV
            salt = new byte[16];
            iv = new byte[12];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            rng.GetBytes(iv);

            // Derive key using PBKDF2
            using var deriveBytes = new Rfc2898DeriveBytes(
                _options.Passkey, salt, 10000, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);

            // Encrypt the entire payload
            using var aes = new AesGcm(key, 16);

            byte[] ciphertext = new byte[payload.Length];
            authTag = new byte[16];

            aes.Encrypt(iv, payload, ciphertext, authTag);

            dataToWrite = ciphertext;
        }
        else
        {
            dataToWrite = payload;
        }

        // Write encrypted header (salt, IV, auth tag) if encrypted
        if (_options.Encrypt && _options.Passkey != null && _options.Passkey.Length > 0)
        {
            writer.Write(salt);
            writer.Write(iv);
            writer.Write(authTag);
        }

        // Write the encrypted (or plain) payload
        writer.Write(dataToWrite);

        for (int i = 0; i < processedFiles.Count; i++)
        {
            var file = processedFiles[i];
            var entry = new PackEntry(
                file.VirtualPath,
                file.Offset,
                file.CompressedSize,
                file.UncompressedSize,
                file.Crc32,
                file.Flags
            );
            _entries.Add(entry);
            _entryMap[file.VirtualPath] = entry;
        }
    }

    private class ProcessedFile
    {
        public string VirtualPath;
        public byte[] Data;
        public ulong Offset;
        public ulong CompressedSize;
        public ulong UncompressedSize;
        public uint Crc32;
        public EntryFlags Flags;
        public bool IsCompressed => (Flags & EntryFlags.Compressed) != 0;
    }

    private ProcessedFile ProcessFile(byte[] data, string virtualPath)
    {
        uint crc = CalculateCrc32(data);
        ulong uncompressedSize = (ulong)data.Length;
        byte[] processedData;
        ulong compressedSize;
        EntryFlags flags = EntryFlags.None;

        bool shouldCompress = _options.CompressionLevel != CompressionLevel.NoCompression;

        if (shouldCompress)
        {
            byte[] compressed = Compress(data);
            compressedSize = (ulong)compressed.Length;

            // Adaptive compression: only use compression if it actually reduces size
            if (_options.AdaptiveCompression && compressedSize >= uncompressedSize)
            {
                processedData = data;
                compressedSize = uncompressedSize;
                flags = EntryFlags.None;
            }
            else
            {
                processedData = compressed;
                flags = EntryFlags.Compressed;
            }
        }
        else
        {
            processedData = data;
            compressedSize = uncompressedSize;
            flags = EntryFlags.None;
        }

        return new ProcessedFile
        {
            VirtualPath = virtualPath,
            Data = processedData,
            UncompressedSize = uncompressedSize,
            CompressedSize = compressedSize,
            Crc32 = crc,
            Flags = flags,
            Offset = 0 // Will be set later
        };
    }

    private byte[] Compress(byte[] data)
    {
        using var outputStream = new MemoryStream();
        using var deflateStream = new System.IO.Compression.DeflateStream(outputStream, GetCompressionLevel(), true);
        deflateStream.Write(data, 0, data.Length);
        deflateStream.Close();
        return outputStream.ToArray();
    }

    private System.IO.Compression.CompressionLevel GetCompressionLevel()
    {
        return _options.CompressionLevel switch
        {
            CompressionLevel.NoCompression => System.IO.Compression.CompressionLevel.NoCompression,
            CompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
            CompressionLevel.Maximum => System.IO.Compression.CompressionLevel.Optimal,
            _ => System.IO.Compression.CompressionLevel.Optimal
        };
    }

    private static uint CalculateCrc32(byte[] data)
    {
        var crc32 = new Crc32();
        crc32.Append(data);
        return BitConverter.ToUInt32(crc32.GetCurrentHash());
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    public void Dispose()
    {
        _entries.Clear();
        _entryMap.Clear();
        _fileData.Clear();
        _filePaths.Clear();
    }
}
