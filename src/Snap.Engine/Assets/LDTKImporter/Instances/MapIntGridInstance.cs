namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents a single int grid cell instance in a tile-based map.
/// Contains grid index data commonly used for collision or logic layers.
/// </summary>
public sealed class MapIntGridInstance : IMapInstance
{
	/// <summary>
	/// The raw integer value assigned to this grid cell. Typically corresponds to a label or behavior.
	/// </summary>
	public int Index { get; }

	/// <summary>
	/// Determines whether the cell is considered solid based on its index.
	/// </summary>
	public bool IsSolid => Index > 0;

	/// <inheritdoc />
	public Vect2 Location {get;}

	/// <inheritdoc />
	public Vect2 Position {get;}

	internal MapIntGridInstance(int index, Vect2 location, Vect2 position)
	{
		Index = index;
		Location = location;
		Position = position;
	}

	internal static List<IMapInstance> Process(JsonElement e, Vect2 gridSize)
	{
		var result = new List<IMapInstance>(e.GetArrayLength());
		var index = 0;

		foreach (var t in e.EnumerateArray())
		{
			var location = new Vect2(index % (int)gridSize.X, index / (int)gridSize.X);
			var position = gridSize * location;

			result.Add(new MapIntGridInstance(t.GetInt32(), location, position));

			index++;
		}

		return result;
	}
}
