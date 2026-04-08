namespace Snap.Engine.Saves;

/// <summary>
/// Contains metadata for serialized content, including checksum, compression status, timestamp, and version.
/// </summary>
public sealed class ContentTypeWriterReaderMetadata
{
	/// <summary>
	/// Gets the hexadecimal checksum of the serialized data.
	/// </summary>
	public string Checksum { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the serialized data is compressed.
	/// </summary>
	public bool IsCompressed { get; internal set; }

	/// <summary>
	/// Gets the UTC timestamp when the data was serialized.
	/// </summary>
	public DateTime Timestamp { get; internal set; }

	/// <summary>
	/// Gets the version of the serialized data format.
	/// </summary>
	public int Version { get; internal set; }

	internal ContentTypeWriterReaderMetadata() { }

	/// <summary>
	/// Creates a new instance of <see cref="ContentTypeWriterReaderMetadata"/> for the given data.
	/// </summary>
	/// <param name="data">The data to generate metadata for.</param>
	/// <param name="isCompressed">Whether the data is compressed.</param>
	/// <param name="version">The version of the serialized data format. Defaults to 1.</param>
	/// <returns>A new <see cref="ContentTypeWriterReaderMetadata"/> instance.</returns>
	public static ContentTypeWriterReaderMetadata Create(byte[] data, bool isCompressed, int version = 1)
	{
		return new ContentTypeWriterReaderMetadata
		{
			Checksum = CompueteChecksumHex(data),
			IsCompressed = isCompressed,
			Timestamp = DateTime.Now,
			Version = version
		};
	}

	/// <summary>
	/// Computes a hexadecimal MD5 checksum for the given data.
	/// </summary>
	/// <param name="data">The data to compute the checksum for.</param>
	/// <returns>The checksum as a hexadecimal string, prefixed with "0x".</returns>
	public static string CompueteChecksumHex(byte[] data)
	{
		byte[] hash = MD5.HashData(data);
		return $"0x{Convert.ToHexString(hash).ToUpper()}";
	}
}

/// <summary>
/// Provides abstract base functionality for reading and writing content of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of content to serialize/deserialize.</typeparam>
public abstract class ContentTypeWriterReader<T>
{
	private readonly byte[] _magicHaeder = [0x53, 0x4E, 0x41, 0x50]; // SNAP
	private readonly string _encryptionKey;

	/// <summary>
	/// Gets the metadata associated with the last save/load operation.
	/// </summary>
	public ContentTypeWriterReaderMetadata Metadata { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ContentTypeWriterReader{T}"/> class.
	/// </summary>
	/// <param name="encryptionKey">Optional encryption key. If <see langword="null"/>, data will not be encrypted.</param>
	protected ContentTypeWriterReader(string encryptionKey = null)
	{
		_encryptionKey = encryptionKey;
	}

	/// <summary>
	/// When implemented in a derived class, writes the specified value to the provided writer.
	/// </summary>
	/// <param name="value">The value to write.</param>
	/// <param name="writer">The writer to use for serialization.</param>
	public abstract void Write(T value, ContentTypeWriter writer);

	/// <summary>
	/// When implemented in a derived class, reads a value of type <typeparamref name="T"/> from the provided reader.
	/// </summary>
	/// <param name="reader">The reader to use for deserialization.</param>
	/// <returns>The deserialized value.</returns>
	public abstract T Read(ContentTypeReader reader);

	/// <summary>
	/// Saves the specified value to a file.
	/// </summary>
	/// <param name="filename">The relative filename to save to. Must not be rooted or contain path traversal characters.</param>
	/// <param name="saveFile">The value to save.</param>
	/// <exception cref="UnauthorizedAccessException">Thrown if the filename is rooted or attempts to escape the save directory.</exception>
	public void Save(string filename, T saveFile)
	{
		using MemoryStream ms = new();
		using ContentTypeWriter writer = new(ms);

		// Serialize all data:
		Write(saveFile, writer);
		byte[] rawData = ms.ToArray();

		// Encrpytion
		byte[] encryptionData = EncryptData(rawData);

		// Compresison:
		byte[] compressedData = CompressData(encryptionData);
		bool useCompression = compressedData.Length < encryptionData.Length;
		byte[] finalData = useCompression ? compressedData : encryptionData;

		ContentTypeWriterReaderMetadata metadata =
			ContentTypeWriterReaderMetadata.Create(finalData, useCompression, version: 1);

		using MemoryStream headerStream = new();
		using BinaryWriter headerWriter = new(headerStream);

		headerWriter.Write(_magicHaeder);
		headerWriter.Write(metadata.Checksum);
		headerWriter.Write(metadata.IsCompressed);
		headerWriter.Write(metadata.Timestamp.Ticks);
		headerWriter.Write(metadata.Version);
		headerWriter.Write(finalData);

		File.WriteAllBytes(CreateFinalPath(filename), headerStream.ToArray());
	}

	private string CreateFinalPath(string filename)
	{
		if (Path.IsPathRooted(filename))
			throw new UnauthorizedAccessException("Save filename must be relative.");

		var safeName = filename
			.Replace("../", "")
			.Replace("..\\", "");

		var invalid = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();
		safeName = new string(safeName.Where(c => !invalid.Contains(c)).ToArray());

		var baseDir = Game.Instance.ApplicationSaveFolder;
		var combined = Path.Combine(baseDir, safeName);
		var fullPath = Path.GetFullPath(combined);
		var normalizedBase = Path.GetFullPath(baseDir)
								 .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
								 + Path.DirectorySeparatorChar;

		if (!fullPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
		{
			throw new UnauthorizedAccessException(
				$"Invalid save path: '{filename}'. Cannot escape save directory.");
		}

		// Make 100% sure the save folder exists.
		Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

		return fullPath;
	}

	/// <summary>
	/// Loads a value of type <typeparamref name="T"/> from the specified file.
	/// </summary>
	/// <param name="filename">The relative filename to load from. Must not be rooted or contain path traversal characters.</param>
	/// <returns>The deserialized value.</returns>
	/// <exception cref="Exception">
	/// Thrown if the file is not a valid save file, the checksum is invalid, or the file is corrupted.
	/// </exception>
	public T Load(string filename)
	{
		byte[] fileData = File.ReadAllBytes(CreateFinalPath(filename));

		using MemoryStream memoryStream = new(fileData);
		using BinaryReader reader = new(memoryStream);

		byte[] magic = reader.ReadBytes(4);
		if (!magic.SequenceEqual(_magicHaeder))
			throw new Exception("Invlid save file");

		string checksum = reader.ReadString();
		bool isCompressed = reader.ReadBoolean();
		long timespamp = reader.ReadInt64();
		int version = reader.ReadInt32();
		byte[] rawData = reader.ReadBytes((int)(memoryStream.Length - memoryStream.Position));

		// Verify Intergity
		string computedChecksum = ContentTypeWriterReaderMetadata.CompueteChecksumHex(rawData);
		if (checksum != computedChecksum)
			throw new Exception("Save file corrupted!");

		// Compress if necessary
		byte[] finalData = isCompressed ? DecompressData(rawData) : rawData;

		// decrypt:
		byte[] decryptedData = DecryptData(finalData);

		using MemoryStream ms = new(decryptedData);
		using ContentTypeReader readerContent = new(ms);

		Metadata = new ContentTypeWriterReaderMetadata
		{
			Checksum = checksum,
			IsCompressed = isCompressed,
			Timestamp = new DateTime(timespamp),
			Version = version
		};

		return Read(readerContent);
	}

	private byte[] EncryptData(byte[] rawData)
	{
		if (string.IsNullOrEmpty(_encryptionKey))
			return rawData;

		using Aes aes = Aes.Create();

		aes.Key = GenerateKey(_encryptionKey);
		aes.GenerateIV();

		using ICryptoTransform encryptor = aes.CreateEncryptor();
		using MemoryStream ms = new();

		ms.Write(aes.IV, 0, aes.IV.Length);

		using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

		cryptoStream.Write(rawData, 0, rawData.Length);
		cryptoStream.FlushFinalBlock();

		return ms.ToArray();
	}

	private byte[] DecryptData(byte[] encyptedData)
	{
		if (string.IsNullOrEmpty(_encryptionKey)) return encyptedData;

		using Aes aes = Aes.Create();
		using MemoryStream ms = new(encyptedData);

		byte[] iv = new byte[16];

		ms.Read(iv, 0, iv.Length);

		aes.Key = GenerateKey(_encryptionKey);
		aes.IV = iv;

		using ICryptoTransform decryptor = aes.CreateDecryptor();
		using CryptoStream cryptoStream = new(ms, decryptor, CryptoStreamMode.Read);
		using MemoryStream outputStream = new();

		cryptoStream.CopyTo(outputStream);

		return outputStream.ToArray();
	}

	private byte[] CompressData(byte[] data)
	{
		using MemoryStream output = new();
		using DeflateStream compressionStream = new(output, System.IO.Compression.CompressionLevel.Optimal);

		compressionStream.Write(data, 0, data.Length);
		compressionStream.Flush();

		return output.ToArray();
	}

	private byte[] DecompressData(byte[] data)
	{
		using MemoryStream input = new(data);
		using MemoryStream output = new();
		using DeflateStream decompressStream = new(input, CompressionMode.Decompress);

		decompressStream.CopyTo(output);

		return output.ToArray();
	}

	private static byte[] GenerateKey(string passPhrase) =>
		SHA256.HashData(Encoding.UTF8.GetBytes(passPhrase));
}
