namespace Snap.Engine.Assets.LDTKImporter;

/// <summary>
/// Represents a parsed LDTK project asset, exposing access to levels, layers, entities, and tilesets.
/// Manages internal caches for fast hashed and indexed lookups.
/// </summary>
public sealed class LDTKProject : IAsset
{
	// cachced levels, entities, etc:
	private readonly Dictionary<uint, MapLevel> _levelCacheById = [];
	private readonly Dictionary<uint, MapLevel> _levelCacheByName = [];
	private readonly Dictionary<ulong, MapEntityInstance> _entityCacheById = [];
	private readonly Dictionary<uint, MapLayer> _layerCacheById = [];
	private readonly Dictionary<uint, MapTileset> _tilesetCacheById = [];
	private readonly Dictionary<uint, MapTileset> _tilesetCacheByName = [];

	/// <summary>
	/// Unique identifier for this LDTK project asset.
	/// </summary>
	public uint Id { get; }

	/// <summary>
	/// File path or tag used to locate the LDTK project JSON file.
	/// </summary>
	public string Tag { get; }

	/// <summary>
	/// Indicates whether the project has been successfully loaded into memory.
	/// </summary>
	public bool IsValid { get; private set; }

	/// <summary>
	/// Returns a native resource handle if applicable. Value is implementation-specific.
	/// </summary>
	public uint Handle { get; }

	internal LDTKProject(uint id, string filename)
	{
		Id = id;
		Tag = filename;
	}

	/// <summary>
	/// Destructor to clean up unmanaged resources.
	/// </summary>
	~LDTKProject() => Dispose();

	/// <summary>
	/// Loads the project data and parses levels, entities, layers, and tilesets into memory.
	/// </summary>
	/// <returns>The size in bytes of the loaded file.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the LDTK file is not found.</exception>
	public ulong Load()
	{
		if (IsValid)
			return 0u;

		byte[] bytes;
		using (var s = AssetManager.OpenStream(Tag))
		using (var ms = new MemoryStream())
		{
			s.CopyTo(ms);
			bytes = ms.ToArray();
		}

		var doc = JsonDocument.Parse(bytes);
		var root = doc.RootElement;

		if (!root.TryGetProperty("defs", out var jDefs))
			throw new InvalidOperationException("Unable to find LDtk Defs");
		if (!jDefs.TryGetProperty("tilesets", out var jTilesets))
			throw new InvalidOperationException("Unable to find LDtk Tilesets");
		if (!root.TryGetProperty("defaultGridSize", out var jDefaultGridSize))
			throw new InvalidOperationException("Unable to find LDtk 'DefaultGridSize'.");
		if (!root.TryGetProperty("levels", out var jLevels))
			throw new InvalidOperationException("Unable to find LDtk 'Levels'.");

		var tilesets = MapTileset.Process(jTilesets);
		var levels = MapLevel.Process(jLevels, jDefaultGridSize.GetInt32());

		foreach (var tileset in tilesets)
		{
			var tilesetId = tileset.Id;
			var tilesetName = HashHelpers.Cache32(tileset.Name);

			_tilesetCacheById[tilesetId] = tileset;
			_tilesetCacheByName[tilesetName] = tileset;
		}

		foreach (var level in levels)
		{
			var lvlCacheId = HashHelpers.Cache32(level.Id);
			var lvlCacheName = HashHelpers.Cache32(level.Name);

			_levelCacheById[lvlCacheId] = level;
			_levelCacheByName[lvlCacheName] = level;

			foreach (var layer in level.Layers)
			{
				var layerCache = HashHelpers.Cache32(layer.Id);

				_layerCacheById[layerCache] = layer;

				if (layer.Type != MapLayerType.Entities)
					continue;

				foreach (var entity in layer.InstanceAs<MapEntityInstance>())
				{
					var entityCache = HashHelpers.Cache64(entity.Id);

					_entityCacheById[entityCache] = entity;
				}
			}
		}

		IsValid = true;

		return (ulong)bytes.Length;
	}

	/// <summary>
	/// Unloads the project and clears all caches.
	/// </summary>
	public void Unload()
	{
		if (!IsValid)
			return;

		Dispose();

		IsValid = false;
	}

	/// <summary>
	/// Disposes the project and releases cached objects and resources.
	/// </summary>
	public void Dispose()
	{
		_levelCacheById.Clear();
		_levelCacheByName.Clear();
		_entityCacheById.Clear();
		_layerCacheById.Clear();
		_tilesetCacheById.Clear();
		_tilesetCacheByName.Clear();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");
	}


	#region Entity
	/// <summary>
	/// Retrieves a map entity instance by its original ID string.
	/// </summary>
	/// <param name="id">The entity ID string.</param>
	/// <returns>The matching <see cref="MapEntityInstance"/>.</returns>
	public MapEntityInstance GetEntityById(string id)
	{
		if (id.IsEmpty())
			throw new ArgumentNullException(nameof(id));
		var hash = HashHelpers.Cache64(id);
		if (!_entityCacheById.TryGetValue(hash, out var entity))
			throw new Exception($"Unable to find a entity with the id '{id}'.");

		return entity;
	}

	public bool TryGetEntityById(string id, out MapEntityInstance value)
	{
		try
		{
			value = GetEntityById(id);
			return true;
		}
		catch
		{
			value = null;
			return false;
		}
	}
	#endregion


	#region Layer
	/// <summary>
	/// Retrieves a map layer by its original ID string.
	/// </summary>
	/// <param name="id">The layer ID string.</param>
	/// <returns>The matching <see cref="MapLayer"/>.</returns>
	public MapLayer GetLayerById(string id)
	{
		if (id.IsEmpty())
			throw new ArgumentNullException(nameof(id));
		if (!_layerCacheById.TryGetValue(HashHelpers.Cache32(id), out var layer))
			throw new KeyNotFoundException($"Unable to find a layer with the id '{id}'.");

		return layer;
	}

	public bool TryGetLayerById(string id, out MapLayer value)
	{
		try
		{
			value = GetLayerById(id);
			return true;
		}
		catch
		{
			value = null;
			return false;
		}
	}
	#endregion


	#region Levels
	/// <summary>
	/// Attempts to retrieve a map level using its original string identifier.
	/// </summary>
	/// <param name="id">The string ID assigned to the level in the LDTK project.</param>
	/// <param name="level">
	/// When this method returns, contains the <see cref="MapLevel"/> associated with the specified ID,
	/// or <c>null</c> if no matching level is found.
	/// </param>
	/// <returns>
	/// <c>true</c> if a level with the given ID exists; otherwise, <c>false</c>.
	/// </returns>
	/// <exception cref="Exception">
	/// Thrown if the level cache is empty, indicating that no levels are available to search.
	/// </exception>
	public bool TryGetLevelById(string id, out MapLevel level)
	{
		try
		{
			level = GetLevelById(id);
			return level != null;
		}
		catch
		{
			level = null;
			return false;
		}
	}

	/// <summary>
	/// Retrieves a map level using its original string identifier.
	/// </summary>
	/// <param name="id">The string ID assigned to the level in the LDTK project.</param>
	/// <returns>The matching <see cref="MapLevel"/> if found; otherwise, <c>null</c>.</returns>
	/// <exception cref="Exception">
	/// Thrown if the level cache is empty, indicating that no levels are available to search.
	/// </exception>
	public MapLevel GetLevelById(string id)
	{
		if (id.IsEmpty())
			throw new ArgumentNullException(nameof(id));
		var hash = HashHelpers.Cache32(id);
		if (!_levelCacheById.TryGetValue(hash, out var level))
			throw new KeyNotFoundException($"Unable to find a level with the id '{id}'.");

		return level;
	}

	/// <summary>
	/// Attempts to retrieve a map level by matching its display name.
	/// </summary>
	/// <param name="name">The name of the level to search for.</param>
	/// <param name="level">
	/// When this method returns, contains the <see cref="MapLevel"/> with the specified name,
	/// or <c>null</c> if no matching level is found.
	/// </param>
	/// <returns>
	/// <c>true</c> if a level with the given name exists; otherwise, <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown if <paramref name="name"/> is <c>null</c> or empty.
	/// </exception>
	/// <exception cref="Exception">
	/// Thrown if the level list is uninitialized or empty.
	/// </exception>
	public bool TryGetLevelByName(string name, out MapLevel level)
	{
		try
		{
			level = GetLevelByName(name);
			return level != null;
		}
		catch
		{
			level = null;
			return false;
		}
	}

	/// <summary>
	/// Retrieves a map level by matching its display name.
	/// </summary>
	/// <param name="name">The name of the level to search for.</param>
	/// <returns>The <see cref="MapLevel"/> with the specified name, or <c>null</c> if not found.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown if <paramref name="name"/> is <c>null</c> or empty.
	/// </exception>
	/// <exception cref="Exception">
	/// Thrown if the level list is uninitialized or empty.
	/// </exception>
	public MapLevel GetLevelByName(string name)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Is null or empty");
		var hash = HashHelpers.Cache32(name);
		if (!_levelCacheByName.TryGetValue(hash, out var level))
			throw new KeyNotFoundException($"Unable to find a level with the name '{name}'.");

		return level;
	}
	#endregion


	#region Tileset
	/// <summary>
	/// Retrieves a tileset from the project by its numeric identifier index.
	/// </summary>
	/// <param name="id">The tileset's internal index value, as defined in the LDTK project.</param>
	/// <returns>The <see cref="MapTileset"/> associated with the given index.</returns>
	/// <exception cref="Exception">
	/// Thrown if the tileset cache is uninitialized or if no tileset matches the specified index.
	/// </exception>
	public MapTileset GetTilesetId(uint id)
	{
		if (!_tilesetCacheById.TryGetValue(id, out var tilemap))
			throw new Exception($"Unable to find a tileset with the id '{id}'.");

		return tilemap;
	}
	public bool TryGetTilesetId(uint id, out MapTileset value)
	{
		try
		{
			value = GetTilesetId(id);
			return true;
		}
		catch
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Retrieves a tileset from the project by matching its name.
	/// </summary>
	/// <param name="name">The name of the tileset as defined in the project.</param>
	/// <param name="ignoreCase">
	/// If <c>true</c>, performs a case-insensitive comparison; otherwise, name matching is case-sensitive.
	/// </param>
	/// <returns>The matching <see cref="MapTileset"/> instance.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown if <paramref name="name"/> is null or an empty string.
	/// </exception>
	/// <exception cref="Exception">
	/// Thrown if the tileset cache is uninitialized or no tileset with the given name is found.
	/// </exception>
	public MapTileset GetTilesetByName(string name)
	{
		if (name.IsEmpty())
			throw new ArgumentNullException(nameof(name), "Is null or empty");
		var hash = HashHelpers.Cache32(name);
		if (!_tilesetCacheByName.TryGetValue(hash, out var tileset))
			throw new Exception($"Unable to find a tileset with the name '{name}'.");

		return tileset;
	}
	public bool TryGetTilesetByName(string name, out MapTileset value)
	{
		try
		{
			value = GetTilesetByName(name);
			return true;
		}
		catch
		{
			value = null;
			return false;
		}
	}
	#endregion
}
