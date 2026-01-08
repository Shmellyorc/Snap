namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents a generic instance placed on the map, defined by its grid location and world-space position.
/// Serves as the base class for tiles, entities, int grid cells, and other map elements.
/// </summary>
public interface IMapInstance
{
	/// <summary>
	/// The logical grid location of the instance within the level or layer, typically in tile units.
	/// </summary>
	Vect2 Location { get; }

	/// <summary>
	/// The world-space pixel position of the instance on the map.
	/// </summary>
	Vect2 Position { get; }
}
