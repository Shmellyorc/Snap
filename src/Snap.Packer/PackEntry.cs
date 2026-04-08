namespace Snap.Packer;

public enum PackFlags : ushort
{
    None = 0,
    Encrypted = 1 << 0,
    Compressed = 1 << 1,
    HasManifest = 1 << 2
}

public struct PackEntry
{
    public string VirtualPath;
    public ulong Offset;
    public ulong CompressedSize;
    public ulong UncompressedSize;
    public uint Crc32;
    public EntryFlags Flags;

    public PackEntry(string virtualPath, ulong offset, ulong compressedSize, ulong uncompressedSize, uint crc32, EntryFlags flags)
    {
        VirtualPath = virtualPath;
        Offset = offset;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;
        Crc32 = crc32;
        Flags = flags;
    }

    public readonly bool IsCompressed => (Flags & EntryFlags.Compressed) != 0;
}
