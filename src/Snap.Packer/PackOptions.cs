namespace Snap.Packer;

public enum CompressionLevel
{
    NoCompression = 0,
    Fastest = 1,
    Default = 6,
    Maximum = 9
}

[Flags]
public enum EntryFlags : ushort
{
    None = 0,
    Compressed = 1 << 0
}

public sealed class PackOptions
{
    public bool Encrypt { get; set; } = false;
    public byte[] Passkey { get; set; } = null;
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Default;
    public bool AdaptiveCompression { get; set; } = true;
    public bool EmbedManifest { get; set; } = true;

    public PackOptions() { }

    public PackOptions(bool encrypt, byte[] passkey, CompressionLevel compression, bool adaptiveCompression, bool embedManifest)
    {
        Encrypt = encrypt;
        Passkey = passkey;
        CompressionLevel = compression;
        AdaptiveCompression = adaptiveCompression;
        EmbedManifest = embedManifest;
    }
}
