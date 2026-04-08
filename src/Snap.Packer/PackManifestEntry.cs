namespace Snap.Packer;

public sealed class PackManifestEntry
{
    public string VirtualPath { get; set; }
    public ulong UncompressedSize { get; set; }
    public ulong CompressedSize { get; set; }
    public uint Crc32 { get; set; }
    public bool Compressed { get; set; }
}
