namespace Snap.Engine.Assets.Mounts;

/// <summary>
/// Represents a mount point that provides access to files through a virtual path system.
/// Mounts are ordered by priority, with earlier mounts taking precedence.
/// </summary>
public interface IMount
{
    /// <summary>Gets the display name of this mount point.</summary>
    string Name { get; }

    /// <summary>Determines whether a file exists at the specified virtual path.</summary>
    /// <param name="virtualPath">The virtual path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool HasFile(string virtualPath);

    /// <summary>Reads the entire contents of the file at the specified virtual path.</summary>
    /// <param name="virtualPath">The virtual path to the file.</param>
    /// <returns>The raw byte data of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    byte[] ReadFile(string virtualPath);
}
