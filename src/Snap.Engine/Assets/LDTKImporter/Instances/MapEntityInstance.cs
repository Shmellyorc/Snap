namespace Snap.Engine.Assets.LDTKImporter.Instances;

/// <summary>
/// Represents a single entity instance placed on a map.
/// Derived from parsed LDTK level data and includes metadata, dimensions, pivot, and custom settings.
/// </summary>
public sealed class MapEntityInstance : IMapInstance
{
	/// <summary>
	/// The unique name or type identifier of this entity as defined in the level data.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The pivot point of the entity, typically expressed in normalized (0–1) coordinates.
	/// </summary>
	public Vect2 Pivot { get; }

	/// <summary>
	/// The unique instance identifier assigned by the level editor.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// The entity's size in tiles or pixels (depending on context).
	/// </summary>
	public Vect2 Size { get; }

	/// <summary>
	/// The world-space coordinates of the entity, typically based on the level grid.
	/// </summary>
	public Vect2 Coords { get; }

	/// <summary>
	/// A collection of tag labels associated with this entity instance.
	/// </summary>
	public List<string> Tags { get; }

	/// <summary>
	/// The width of the entity based on its <see cref="Size"/> component.
	/// </summary>
	public float Width => Size.X;

	/// <summary>
	/// The height of the entity based on its <see cref="Size"/> component.
	/// </summary>
	public float Height => Size.Y;

	/// <summary>
	/// A map of field values (custom user-defined settings) attached to this entity instance.
	/// </summary>
	public Dictionary<uint, MapSetting> Settings { get; }

	/// <inheritdoc />
	public Vect2 Location { get; }

	/// <inheritdoc />
	public Vect2 Position { get; }

	/// <summary>
	/// Converts tag strings into enum values of the specified type.
	/// </summary>
	/// <typeparam name="TEnum">The enum type to convert to.</typeparam>
	/// <returns>A list of parsed enum values matching the current tags.</returns>
	public List<TEnum> TagsAs<TEnum>() where TEnum : Enum
	{
		var result = new List<TEnum>(Tags.Count);

		for (int i = 0; i < Tags.Count; i++)
		{
			var tag = Tags[i];

			if (!Enum.TryParse(typeof(TEnum), tag, true, out var eResult))
				continue;

			result.Add((TEnum)eResult);
		}

		return result;
	}

	internal MapEntityInstance(string name, Vect2 pivot, string id, Vect2 size,
		Vect2 coords, List<string> tags, Vect2 location, Vect2 position,
		Dictionary<uint, MapSetting> settings)
	{
		Name = name;
		Pivot = pivot;
		Id = id;
		Size = size;
		Coords = coords;
		Tags = tags;
		Settings = settings;
		Location = location;
		Position = position;
	}

	internal static List<IMapInstance> Process(JsonElement e)
	{
		var result = new List<IMapInstance>(e.GetArrayLength());

		foreach (var t in e.EnumerateArray())
		{
			var name = t.GetPropertyOrDefault("__identifier", string.Empty);
			var location = t.GetPosition("__grid");
			var pivot = t.GetPosition("__pivot");
			var id = t.GetPropertyOrDefault("iid", string.Empty);
			var cX = t.GetPropertyOrDefault<int>("width");
			var cY = t.GetPropertyOrDefault<int>("height");
			var position = t.GetPosition("px");
			var worldX = t.GetPropertyOrDefault<int>("__worldX");
			var worldY = t.GetPropertyOrDefault<int>("__worldY");
			var tags = t.GetProperty("__tags")
				.EnumerateArray()
				.Where(x => x.ValueKind != JsonValueKind.Null)
				.Select(x => x.GetString()!)
				.ToList();

			var settings = JsonHelpers.GetSettings(t.GetProperty("fieldInstances"));

			result.Add(new MapEntityInstance(name, pivot, id, new(cX, cY),
				new(worldX, worldY), tags, location, position, settings));
		}

		return result;
	}
}
