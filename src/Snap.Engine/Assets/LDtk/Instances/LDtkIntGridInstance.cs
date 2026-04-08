namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents a single int grid cell instance in a tile-based map.
/// Contains grid index data commonly used for collision or logic layers.
/// </summary>
public sealed class LDtkIntGridInstance : ILDtkInstance
{
	/// <summary>
	/// The raw integer value assigned to this grid cell. Typically corresponds to a label or behavior.
	/// </summary>
	public int Index { get; }

	/// <summary>Converts the index value to the specified enum type.</summary>
	/// <typeparam name="T">The enum type to convert to.</typeparam>
	/// <returns>The enum value corresponding to the index.</returns>
	public T IndexAsEnum<T>() where T : Enum => (T)Enum.ToObject(typeof(T), Index);

	/// <summary>
	/// Determines whether the cell is considered solid based on its index.
	/// </summary>
	public bool IsSolid => Index > 0;

	/// <inheritdoc />
	public Vect2 Location { get; }

	/// <inheritdoc />
	public Vect2 Position { get; }

	internal LDtkIntGridInstance(int index, Vect2 location, Vect2 position)
	{
		Index = index;
		Location = location;
		Position = position;
	}

	internal static List<ILDtkInstance> Process(JsonElement e, Vect2 gridSize)
	{
		var result = new List<ILDtkInstance>(e.GetArrayLength());
		var index = 0;

		foreach (var t in e.EnumerateArray())
		{
			var location = new Vect2(index % (int)gridSize.X, index / (int)gridSize.X);
			var position = gridSize * location;

			result.Add(new LDtkIntGridInstance(t.GetInt32(), location, position));

			index++;
		}

		return result;
	}
}
