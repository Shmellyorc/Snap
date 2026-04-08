namespace Snap.Engine.Assets.Mounts;

/// <summary>
/// Mount point for a packed archive file (.pack) that provides virtual file access with optional encryption.
/// Implements <see cref="IDisposable"/> to release the underlying pack reader resources.
/// </summary>
/// <remarks>
/// Pack mounts allow bundling multiple assets into a single encrypted or unencrypted archive,
/// reducing file fragmentation and optionally protecting asset data.
/// </remarks>
public sealed class PackMount : IMount, IDisposable
{
    private PackReader _reader;
    private readonly string _packPath;

    /// <summary>Gets the display name of this mount, derived from the pack file name.</summary>
    public string Name => Path.GetFileName(_packPath);

    /// <summary>
    /// Initializes a new pack mount from the specified file path, optionally with an encryption key.
    /// </summary>
    /// <param name="packPath">The full physical path to the pack file.</param>
    /// <param name="key">Optional encryption key bytes. If <c>null</c>, the pack is treated as unencrypted.</param>
    public PackMount(string packPath, byte[] key = null)
    {
        _packPath = packPath;
        _reader = new PackReader(packPath, key);
    }

    /// <summary>Determines whether a file exists at the specified virtual path within the pack.</summary>
    /// <param name="virtualPath">The virtual path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public bool HasFile(string virtualPath) => _reader.HasFile(virtualPath);

    /// <summary>Reads the entire contents of the file at the specified virtual path within the pack.</summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <returns>The raw byte data of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    public byte[] ReadFile(string virtualPath) => _reader.ReadFile(virtualPath);

    /// <summary>Releases the underlying pack reader resources.</summary>
    public void Dispose()
    {
        _reader?.Dispose();
        _reader = null;
    }
}
