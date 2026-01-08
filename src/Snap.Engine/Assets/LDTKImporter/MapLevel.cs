namespace Snap.Engine.Assets.LDTKImporter;

/// <summary>
/// Represents a single level within an LDTK-based map.
/// Contains level metadata, spatial dimensions, background styling, associated layers, and custom settings.
/// </summary>
public sealed class MapLevel
{
	/// <summary>
	/// The display name of the level, as assigned in the source project.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The unique instance identifier for this level.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The pixel-based world position of the level's top-left corner.
	/// </summary>
	public Vect2 Coords { get; }

	/// <summary>
	/// The relative depth or ordering of the level in the world.
	/// Often used to sort level instances visually or logically.
	/// </summary>
	public int WorldDepth { get; }

	/// <summary>
	/// The size of the level in pixels (width × height).
	/// </summary>
	public Vect2 Size { get; }

	/// <summary>
	/// The size of the level in grid units, derived from its pixel dimensions and tile size.
	/// </summary>
	public Vect2 GridSize { get; }

	/// <summary>
	/// The background color assigned to this level, parsed from LDTK.
	/// </summary>
	public Color Color { get; }

	/// <summary>
	/// The relative file path to the background image, if one is defined.
	/// </summary>
	public string BgPath { get; }

	/// <summary>
	/// The pixel position at which the background image is rendered.
	/// </summary>
	public Vect2 BgPosition { get; }

	/// <summary>
	/// The normalized pivot point used for aligning the background image (0.0 to 1.0).
	/// </summary>
	public Vect2 BgPivot { get; }

	/// <summary>
	/// Gets the neighboring map level references for this level.
	/// </summary>
	/// <remarks>
	/// This provides access to the IDs of adjacent levels in each compass direction (N, NE, E, etc.).
	/// Use this to determine map transitions or connectivity.
	/// </remarks>
	public MapNeighbour Neighbours { get; }

	/// <summary>
	/// A collection of all layers contained within the level.
	/// Layers include tilemaps, entities, and int grids.
	/// </summary>
	public IReadOnlyList<MapLayer> Layers { get; }

	/// <summary>
	/// A dictionary of user-defined custom field values attached to the level.
	/// Keys are hashed field identifiers mapped to typed <see cref="MapSetting"/> entries.
	/// </summary>
	public IReadOnlyDictionary<uint, MapSetting> Settings { get; }

	internal MapLevel(string name, string id, Vect2 coords, int worthDepth, Vect2 size,
		Vect2 gridSize, Color color, string bgPath, Vect2 bgPosition, Vect2 bgPivot,
		MapNeighbour neighbours, List<MapLayer> layers, Dictionary<uint, MapSetting> settings)
	{
		Name = name;
		Id = id;
		Coords = coords;
		WorldDepth = worthDepth;
		Size = size;
		GridSize = gridSize;
		Color = color;
		BgPath = bgPath;
		BgPosition = bgPosition;
		BgPivot = bgPivot;
		Neighbours = neighbours;
		Layers = layers;
		Settings = settings;
	}

	internal static List<MapLevel> Process(JsonElement e, int tileSize)
	{
		var result = new List<MapLevel>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			Color color;
			var name = t.GetPropertyOrDefault("identifier", string.Empty);
			var id = t.GetPropertyOrDefault("iid", string.Empty);
			var worldX = t.GetPropertyOrDefault<int>("worldX");
			var worldY = t.GetPropertyOrDefault<int>("worldY");
			var worldDepth = t.GetPropertyOrDefault<int>("worldDepth");
			var pxX = t.GetPropertyOrDefault<int>("pxWid");
			var pxY = t.GetPropertyOrDefault<int>("pxHei");
			var bgRelPath = t.GetPropertyOrDefault("bgRelPath", string.Empty);
			var bgPivotX = t.GetPropertyOrDefault<float>("bgPivotX");
			var bgPivotY = t.GetPropertyOrDefault<float>("bgPivotY");
			var size = new Vect2(pxX, pxY);
			var gridSize = Vect2.Floor(size / tileSize);
			var pxBgPos = Vect2.Zero;

			// Note: Uses __bgColor if the LDTK map color is unset; otherwise uses bgColor.
			if (t.TryGetProperty("bgColor", out var bgProp) && bgProp.ValueKind != JsonValueKind.Null)
				color = new Color(t.GetPropertyOrDefault("bgColor", "#ffffff"));
			else // Fallback to __bgColor
				color = new Color(t.GetPropertyOrDefault("__bgColor", "#ffffff"));

			if (t.TryGetProperty("bgPos", out var bgPos) && bgPos.ValueKind != JsonValueKind.Null)
			{
				var bgElem = bgPos.EnumerateArray();
				pxBgPos = new Vect2(bgElem.First().GetInt32(), bgElem.Last().GetInt32());
			}

			var neighbours = MapNeighbour.Process(t.GetProperty("__neighbours"));
			var settings = JsonHelpers.GetSettings(t.GetProperty("fieldInstances"));
			var layers = MapLayer.Process(t.GetProperty("layerInstances"));

			result.Add(
				new MapLevel(name, id, new(worldX, worldY), worldDepth, size, gridSize,
				color, bgRelPath, pxBgPos, new(bgPivotX, bgPivotY), neighbours, layers, settings)
			);
		}

		return result;
	}
}
