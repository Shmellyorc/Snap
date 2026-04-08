namespace Snap.Engine.Assets.LDTKImporter;

/// <summary>
/// Represents a reference to another entity instance within an LDTK project.
/// Encapsulates identifiers required to locate an entity across layers, levels, and worlds.
/// </summary>
public sealed class LDtkEntityRef
{
	/// <summary>
	/// The unique entity ID being referenced.
	/// </summary>
	public string EntityId { get; }

	/// <summary>
	/// The unique ID of the layer containing the referenced entity.
	/// </summary>
	public string LayerId { get; }

	/// <summary>
	/// The unique ID of the level containing the referenced entity.
	/// </summary>
	public string LevelId { get; }

	/// <summary>
	/// The unique ID of the world in which the referenced entity resides.
	/// </summary>
	public string WorldId { get; }

	internal LDtkEntityRef(string entityId, string layerId, string levelId, string worldId)
	{
		EntityId = entityId;
		LayerId = layerId;
		LevelId = levelId;
		WorldId = worldId;
	}

	internal static LDtkEntityRef Process(JsonElement e)
	{
		var entityId = e.GetPropertyOrDefault("entityIid", string.Empty);
		var layerId = e.GetPropertyOrDefault("layerIid", string.Empty);
		var levelId = e.GetPropertyOrDefault("levelIid", string.Empty);
		var worldId = e.GetPropertyOrDefault("worldIid", string.Empty);

		return new LDtkEntityRef(entityId, layerId, levelId, worldId);
	}
}
