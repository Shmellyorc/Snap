namespace Snap.Engine.Assets.Loaders;

/// <summary>
/// Represents a generic asset managed by the engine.
/// Defines a consistent interface for loading, unloading, identification, and disposal.
/// </summary>
public interface IAsset : IDisposable
{
	/// <summary>
	/// A unique identifier assigned to the asset at creation or load time.
	/// Used internally by the asset manager to distinguish loaded resources.
	/// </summary>
	uint Id { get; }

	/// <summary>
	/// A tag or path that provides semantic context for the asset,
	/// such as its source file or runtime alias.
	/// </summary>
	string Tag { get; }

	/// <summary>
	/// Indicates whether the asset has been successfully loaded and is valid for use.
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// A graphics or system handle associated with the asset, typically for binding or GPU use.
	/// </summary>
	uint Handle { get; }


	DateTime LastAccessFrame { get; }


	ulong Length { get; }

	/// <summary>
	/// Loads the asset into memory and returns its byte size.
	/// May trigger file I/O, deserialization, or graphics API registration.
	/// </summary>
	/// <returns>
	/// The number of bytes consumed by the asset after loading.
	/// </returns>
	ulong Load();

	/// <summary>
	/// Releases memory or handles used by the asset without removing it from the registry.
	/// </summary>
	void Unload();

	// /// <summary>
	// /// Completely disposes of the asset, releasing all unmanaged resources.
	// /// </summary>
	// void Dispose();
}
