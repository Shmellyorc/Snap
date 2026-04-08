namespace Snap.Engine.Assets.Mounts;

/// <summary>
/// Mount point for the virtual file system that maps virtual paths to physical files within the configured content root.
/// Serves as the fallback mount when no other mounts provide the requested file.
/// </summary>
/// <remarks>
/// This mount enforces security by ensuring all resolved paths remain within the content root directory,
/// preventing directory traversal attacks.
/// </remarks>
public sealed class VirtualFileSystemMount : IMount
{
    /// <summary>Gets the display name of this mount.</summary>
    public string Name => "Virtual File System";

    /// <summary>Determines whether a file exists at the specified virtual path within the content root.</summary>
    /// <param name="virtualPath">The virtual path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public bool HasFile(string virtualPath)
    {
        string fullPath = GetFullPath(virtualPath);
        return File.Exists(fullPath);
    }

    /// <summary>Reads the entire contents of the file at the specified virtual path within the content root.</summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <returns>The raw byte data of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the resolved path falls outside the content root.</exception>
    public byte[] ReadFile(string virtualPath)
    {
        string fullPath = GetFullPath(virtualPath);
        return File.ReadAllBytes(fullPath);
    }

    private string GetFullPath(string virtualPath)
    {
        string contentRoot = EngineSettings.Instance.AppContentRoot;

        if (!contentRoot.EndsWith('/') && !contentRoot.EndsWith('\\'))
            contentRoot += Path.DirectorySeparatorChar;

        string fullPath = Path.GetFullPath(Path.Combine(contentRoot, virtualPath));

        if (!fullPath.StartsWith(Path.GetFullPath(contentRoot)))
            throw new UnauthorizedAccessException($"Cannot access file outside of ContentRoot: {virtualPath}");

        return fullPath;
    }
}
