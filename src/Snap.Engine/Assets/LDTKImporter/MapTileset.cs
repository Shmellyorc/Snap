namespace Snap.Engine.Assets.LDTKImporter;

/// <summary>
/// Represents a tileset used by one or more layers in an LDTK project.
/// Encapsulates metadata and layout details for referencing individual tiles in rendering or logic.
/// </summary>
public sealed class MapTileset
{
	/// <summary>
	/// The unique numeric ID assigned to the tileset in the source project.
	/// </summary>
	public uint Id { get; }

	/// <summary>
	/// The name of the tileset, as defined in the LDTK editor.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The dimensions of the grid layout in number of cells (columns × rows).
	/// </summary>
	public Vect2 CellSize { get; }

	/// <summary>
	/// The pixel dimensions of the full tileset texture (width × height).
	/// </summary>
	public Vect2 Size { get; }

	/// <summary>
	/// The relative file path to the tileset image used by map layers.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// The size of a single tile within the tileset, in pixels.
	/// </summary>
	public int TileSize { get; }

	/// <summary>
	/// The horizontal and vertical spacing between tiles, in pixels.
	/// </summary>
	public int Spacing { get; }

	/// <summary>
	/// The pixel padding around the outer edges of the tileset.
	/// </summary>
	public int Padding { get; }

	/// <summary>
	/// A list of user-defined string tags assigned to the tileset or specific tiles.
	/// Often used to drive rules, filters, or behaviors.
	/// </summary>
	public List<string> Tags { get; }

	internal MapTileset(uint id, string name, Vect2 cellSize, Vect2 size,
		string path, int tileSize, int spacing, int padding, List<string> tags)
	{
		Id = id;
		Name = name;
		CellSize = cellSize;
		Size = size;
		Path = path;
		TileSize = tileSize;
		Spacing = spacing;
		Padding = padding;
		Tags = tags;
	}

	internal static List<MapTileset> Process(JsonElement e)
	{
		var result = new List<MapTileset>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var cWidth = t.GetPropertyOrDefault<int>("__cWid");
			var cHeight = t.GetPropertyOrDefault<int>("__cHei");
			var name = t.GetPropertyOrDefault<string>("identifier");
			var id = t.GetPropertyOrDefault<uint>("uid");
			var path = t.GetPropertyOrDefault("relPath", string.Empty);
			var pxWid = t.GetPropertyOrDefault<int>("pxWid");
			var pxHei = t.GetPropertyOrDefault<int>("pxHei");
			var tileSize = t.GetPropertyOrDefault<int>("tileGridSize");
			var spacing = t.GetPropertyOrDefault<int>("spacing");
			var padding = t.GetPropertyOrDefault<int>("padding");
			var tags = t.GetProperty("enumTags").EnumerateArray()
				.Where(x => x.ValueKind != JsonValueKind.Null)
				.Select(x => x.GetString()!)
				.ToList();

			result.Add(
				new MapTileset(
					id,
					name,
					new(cWidth, cHeight),
					new(pxWid, pxHei),
					path,
					tileSize,
					spacing,
					padding,
					tags
				)
			);
		}

		return result;
	}
}
