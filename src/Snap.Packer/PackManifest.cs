namespace Snap.Packer;

public sealed class PackManifest
{
    public string Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FileCount { get; set; }
    public List<PackManifestEntry> Files { get; set; } = new();
}

