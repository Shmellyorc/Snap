namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents an individual tile placed on a map layer.
/// Includes visual frame data, flipping effects, alpha transparency, and grid metadata.
/// </summary>
public sealed class MapTileInstance : IMapInstance
{
	/// <summary>
	/// The source rectangle within the tileset texture that defines this tile's graphic.
	/// </summary>
	public Rect2 Source { get; }

	/// <summary>
	/// Optional texture transformations such as horizontal or vertical flipping.
	/// </summary>
	public TextureEffects Effects { get; }

	/// <summary>
	/// The unique tile identifier, typically referencing a tileset index.
	/// </summary>
	public int Tile { get; }

	/// <summary>
	/// The alpha transparency level for the tile. Ranges from 0.0 (fully transparent) to 1.0 (opaque).
	/// </summary>
	public float Alpha { get; }

	/// <inheritdoc />
	public Vect2 Location { get; }

	/// <inheritdoc />
	public Vect2 Position { get; }

	internal MapTileInstance(Rect2 source, TextureEffects effects, int tile, float alpha,
		Vect2 location, Vect2 position)
	{
		Source = source;
		Effects = effects;
		Tile = tile;
		Alpha = alpha;
		Location = location;
		Position = position;
	}

	internal static List<IMapInstance> Process(JsonElement e, int tileSize)
	{
		var result = new List<IMapInstance>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var position = t.GetPosition("px");
			var src = t.GetPosition("src");
			var flag = t.GetPropertyOrDefault<int>("f");
			var tile = t.GetPropertyOrDefault<int>("t");
			var alpha = t.GetPropertyOrDefault<float>("a");
			var location = Vect2.Floor(position / tileSize);
			var srcRect = new Rect2(src, new(tileSize));

			TextureEffects effects = flag switch
			{
				1 => TextureEffects.FlipHorizontal,
				2 => TextureEffects.FlipVertical,
				3 => TextureEffects.FlipHorizontal | TextureEffects.FlipVertical,
				_ => TextureEffects.None
			};

			result.Add(new MapTileInstance(srcRect, effects, tile, alpha, location, position));
		}

		return result;
	}
}
