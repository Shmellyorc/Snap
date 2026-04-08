namespace Snap.Engine.Assets.Mounts;

/// <summary>
/// Mount point for macOS application bundle resources. Provides access to files within the app bundle's
/// Resources folder. Only available on macOS platforms.
/// </summary>
/// <remarks>
/// When running within a bundled .app, the mount resolves to Contents/Resources.
/// In development environments, it falls back to the configured content root.
/// </remarks>
public sealed class MacOsMount : IMount
{
    /// <summary>Gets the display name of this mount.</summary>
    public string Name => "macOS Bundle";

    private readonly string _resourcesPath;

    /// <summary>
    /// Initializes a new macOS bundle mount. Throws <see cref="PlatformNotSupportedException"/>
    /// if called on a non-macOS platform.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when not running on macOS.</exception>
    public MacOsMount()
    {
        // Only works on macOS
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            throw new PlatformNotSupportedException("macOSMount is only supported on macOS");

        // Get the bundle's Resources folder
        string bundlePath = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate to Contents/Resources if we're in a .app bundle
        if (bundlePath.Contains(".app/Contents/MacOS"))
        {
            _resourcesPath = bundlePath.Replace("MacOS", "Resources");
        }
        else
        {
            // Development environment - fallback to VFS
            _resourcesPath = EngineSettings.Instance.AppContentRoot;
        }

        // Ensure path exists
        if (!Directory.Exists(_resourcesPath))
            _resourcesPath = EngineSettings.Instance.AppContentRoot;
    }

    /// <summary>Determines whether a file exists at the specified virtual path within the macOS bundle.</summary>
    /// <param name="virtualPath">The virtual path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    public bool HasFile(string virtualPath)
    {
        string fullPath = Path.Combine(_resourcesPath, virtualPath);
        return File.Exists(fullPath);
    }

    /// <summary>Reads the entire contents of the file at the specified virtual path within the macOS bundle.</summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <returns>The raw byte data of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    public byte[] ReadFile(string virtualPath)
    {
        string fullPath = Path.Combine(_resourcesPath, virtualPath);
        return File.ReadAllBytes(fullPath);
    }
}
