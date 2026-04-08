#pragma warning disable CA2022

namespace Snap.Packer;

public sealed class PackReader : IDisposable
{
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly Dictionary<string, PackEntry> _entries = new();
    private readonly PackFlags _flags;
    private readonly byte[] _passkey;
    private readonly bool _isEncrypted;
    private readonly ulong _tableOffset;
    private readonly uint _fileCount;
    private readonly long _dataStartOffset;
    private readonly Stream _dataStream;
    private readonly BinaryReader _dataReader;

    public PackReader(string packPath, byte[] passkey = null)
    {
        _stream = new FileStream(packPath, FileMode.Open, FileAccess.Read);
        _reader = new BinaryReader(_stream);
        _passkey = passkey;

        // Read header
        uint magic = _reader.ReadUInt32();
        if (magic != 0x50595243)
            throw new InvalidDataException("Invalid pack file magic");

        ushort version = _reader.ReadUInt16();
        if (version != 1)
            throw new InvalidDataException($"Unsupported pack version: {version}");

        _flags = (PackFlags)_reader.ReadUInt16();
        _fileCount = _reader.ReadUInt32();
        _tableOffset = _reader.ReadUInt64();

        uint headerCrc = _reader.ReadUInt32();

        // Verify header CRC
        _stream.Position = 0;
        byte[] headerBytes = new byte[20];
        _stream.Read(headerBytes, 0, 20);
        uint calculatedCrc = CalculateCrc32(headerBytes);
        if (calculatedCrc != headerCrc)
            throw new InvalidDataException("Pack header CRC mismatch");

        _isEncrypted = (_flags & PackFlags.Encrypted) != 0;

        byte[] fileTableData;

        if (_isEncrypted)
        {
            if (_passkey == null || _passkey.Length == 0)
                throw new InvalidOperationException("Pack is encrypted but no passkey provided");

            // Move stream position to byte 24 (after the 24-byte header)
            _stream.Position = 24;

            byte[] salt = _reader.ReadBytes(16);
            byte[] iv = _reader.ReadBytes(12);
            byte[] authTag = _reader.ReadBytes(16);

            using var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(
                _passkey, salt, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);
            byte[] encryptedData = _reader.ReadBytes((int)(_stream.Length - _stream.Position));

            using var aes = new System.Security.Cryptography.AesGcm(key, 16);
            byte[] decryptedData = new byte[encryptedData.Length];
            aes.Decrypt(iv, encryptedData, authTag, decryptedData);

            // Parse the decrypted payload
            using var payloadStream = new MemoryStream(decryptedData);
            using var payloadReader = new BinaryReader(payloadStream);

            fileTableData = ReadFileTable(payloadReader, out var dataOffset);

            _dataStream = new MemoryStream(decryptedData);
            _dataReader = new BinaryReader(_dataStream);
            _dataStream.Position = dataOffset;
        }
        else
        {
            // Unencrypted pack - seek to table offset
            _stream.Position = (long)_tableOffset;
            fileTableData = ReadFileTable(_reader, out _);

            // For unencrypted packs, we don't need _dataStartOffset
            // The offsets in the file table are absolute positions
            _dataStartOffset = 0; // Not used, but keep to avoid null reference
        }

        using var ftStream = new MemoryStream(fileTableData);
        using var ftReader = new BinaryReader(ftStream);

        ushort entryCount = ftReader.ReadUInt16();
        for (int i = 0; i < entryCount; i++)
        {
            byte pathLen = ftReader.ReadByte();
            byte[] pathBytes = ftReader.ReadBytes(pathLen);
            string path = System.Text.Encoding.UTF8.GetString(pathBytes);

            ulong offset = ftReader.ReadUInt64();
            ulong compressedSize = ftReader.ReadUInt64();
            ulong uncompressedSize = ftReader.ReadUInt64();
            uint crc = ftReader.ReadUInt32();
            EntryFlags flags = (EntryFlags)ftReader.ReadUInt16();

            _entries[path] = new PackEntry(path, offset, compressedSize, uncompressedSize, crc, flags);
        }
    }

    private byte[] ReadFileTable(BinaryReader reader, out long dataOffset)
    {
        long startPos = reader.BaseStream.Position;
        ushort entryCount = reader.ReadUInt16();

        // Skip to end of file table
        for (int i = 0; i < entryCount; i++)
        {
            byte pathLen = reader.ReadByte();
            reader.BaseStream.Position += pathLen;
            reader.BaseStream.Position += 8 + 8 + 8 + 4 + 2; // offset, compressed, uncompressed, crc, flags
        }

        dataOffset = reader.BaseStream.Position;
        long tableSize = dataOffset - startPos;

        reader.BaseStream.Position = startPos;
        return reader.ReadBytes((int)tableSize);
    }

    public bool HasFile(string virtualPath)
    {
        return _entries.ContainsKey(NormalizePath(virtualPath));
    }

    public PackEntry? GetFileInfo(string virtualPath)
    {
        if (_entries.TryGetValue(NormalizePath(virtualPath), out var entry))
            return entry;
        return null;
    }

    public byte[] ReadFile(string virtualPath)
    {
        if (!_entries.TryGetValue(NormalizePath(virtualPath), out var entry))
            throw new FileNotFoundException($"File not found in pack: {virtualPath}");

        byte[] compressedData;

        if (_isEncrypted)
        {
            if (_dataStream == null)
                throw new InvalidOperationException("Decrypted data stream not available");

            _dataStream.Position = (long)entry.Offset;
            compressedData = _dataReader.ReadBytes((int)entry.CompressedSize);
        }
        else
        {
            // For unencrypted packs, entry.Offset is absolute file position
            _stream.Position = (long)entry.Offset;
            compressedData = _reader.ReadBytes((int)entry.CompressedSize);
        }

        byte[] decompressedData;
        if (entry.IsCompressed)
        {
            decompressedData = Decompress(compressedData, (int)entry.UncompressedSize);
        }
        else
        {
            decompressedData = compressedData;
        }

        // Verify CRC
        uint crc = CalculateCrc32(decompressedData);
        if (crc != entry.Crc32)
            throw new InvalidDataException($"CRC mismatch for file: {virtualPath}");

        return decompressedData;
    }

    public List<string> GetAllFiles()
    {
        return _entries.Keys.ToList();
    }

    private byte[] Decompress(byte[] compressedData, int uncompressedSize)
    {
        using var inputStream = new MemoryStream(compressedData);
        using var deflateStream = new System.IO.Compression.DeflateStream(inputStream, System.IO.Compression.CompressionMode.Decompress);
        using var outputStream = new MemoryStream(uncompressedSize);
        deflateStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }

    private static uint CalculateCrc32(byte[] data)
    {
        var crc32 = new System.IO.Hashing.Crc32();
        crc32.Append(data);
        return BitConverter.ToUInt32(crc32.GetCurrentHash());
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _stream?.Dispose();
        _dataReader?.Dispose();
        _dataStream?.Dispose();
    }
}
