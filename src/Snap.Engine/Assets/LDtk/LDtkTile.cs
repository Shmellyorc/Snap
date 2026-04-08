namespace Snap.Engine.Assets.LDTKImporter;

/// <summary>
/// Represents a single tile selection from a tileset.
/// Used to define visual references for tiles placed on the map or in field metadata.
/// </summary>
public sealed class LDtkTile
{
	/// <summary>
	/// The identifier of the tileset that this tile belongs to.
	/// Matches the unique ID assigned in the LDTK project.
	/// </summary>
	public int TilesetId { get; }

	/// <summary>
	/// The rectangular region within the tileset texture that this tile references, in pixels.
	/// Defines the tile’s source sprite region (X, Y, Width, Height).
	/// </summary>
	public Rect2 Source { get; }

	internal LDtkTile(int tilesetId, Rect2 source)
	{
		TilesetId = tilesetId;
		Source = source;
	}

	internal static LDtkTile Process(JsonElement e)
	{
		var tilesetId = e.GetPropertyOrDefault<int>("tilesetUid");
		var x = e.GetPropertyOrDefault<int>("x");
		var y = e.GetPropertyOrDefault<int>("y");
		var w = e.GetPropertyOrDefault<int>("w");
		var h = e.GetPropertyOrDefault<int>("h");

		return new LDtkTile(tilesetId, new(x, y, w, h));
	}
}
