namespace Snap.Engine.Assets.Loaders;

// Supported types: wav, mp3, ogg, flac, aiff, au, raw, paf, svx, nist, voc, ircam, 
// w64, mat4, mat5, pvf, htk, sds, avr, sd2, caf, wve, mpc2k, rf64.

/// <summary>
/// Represents a sound asset that can be played and reused across multiple instances.
/// Supports buffered loading, playback control, and tracking of active instances.
/// </summary>
public sealed class Sound : IAsset, IEquatable<Sound>
{
	internal SFSoundBuffer Buffer;

	// private readonly List<SoundInstance> _instances = new(32);
	// private readonly object _instanceLock = new();

	/// <summary>
	/// The unique asset ID assigned by the asset manager.
	/// </summary>
	public uint Id { get; }

	/// <summary>
	/// The original file path or tag assigned to the sound asset.
	/// </summary>
	public string Tag { get; }

	/// <summary>
	/// Indicates whether the sound has been successfully loaded and is valid for playback.
	/// </summary>
	public bool IsValid { get; private set; }

	/// <summary>
	/// The internal handle for the asset. Not used for sound directly, but required to satisfy <see cref="IAsset"/>.
	/// </summary>
	public uint Handle => Id; // not really used here but required for textures

	/// <summary>
	/// The duration of the loaded sound in seconds.
	/// Returns <c>TimeSpan.Zero</c> if the asset is not loaded.
	/// </summary>
	public TimeSpan Duration => IsValid ? Buffer.Duration.ToTimeSpan() : TimeSpan.Zero;



	public DateTime LastAccessFrame { get; private set; }


	public ulong Length { get; private set; }



	/// <summary>
	/// The sample rate (in Hz) of the sound buffer. Returns 0 if not loaded.
	/// </summary>
	public uint SampleRate => IsValid ? Buffer.SampleRate : 0u;

	/// <summary>
	/// The number of channels in the sound buffer (e.g., mono or stereo).
	/// Returns <see cref="SoundChannelType.Unknown"/> if not loaded.
	/// </summary>
	public SoundChannelType Channel => IsValid ? (SoundChannelType)Buffer.ChannelCount : SoundChannelType.Unknown;

	/// <summary>
	/// Indicates whether the sound should loop when played.
	/// </summary>
	public bool IsLooped { get; }



	internal Sound(uint id, string filename, bool looped)
	{
		Id = id;
		Tag = filename;
		IsLooped = looped;
	}

	/// <summary>
	/// Finalizer that disposes of unmanaged resources held by this sound asset.
	/// </summary>
	~Sound() => Dispose();

	/// <summary>
	/// Creates a new sound instance from this asset if it is valid and loaded.
	/// Recycles or purges invalid instances before allocation.
	/// </summary>
	/// <returns>
	/// A <see cref="SoundInstance"/> ready for playback, or <c>null</c> if the asset is not valid.
	/// </returns>
	public SoundInstance CreateInstance()
	{
		if (!IsValid)
			Load();

		return SoundInstancePool.GetInstance(this);
	}

	/// <summary>
	/// Attempts to unload the sound buffer if no active sound instances are using it.
	/// Logs a warning if currently in use.
	/// </summary>
	public void Unload()
	{
		if (SoundInstancePool.HasActiveInstances(this))
		{
			Logger.Instance.Log(LogLevel.Warning,
				$"Asset eviction was canceled for ID {Id}, Type: '{GetType().Name}', " +
				$"because it is currently is use as a sound instance."
			);
			return;
		}

		if (!IsValid)
			return;

		SoundInstancePool.OnSoundDispose(this);

		Buffer?.Dispose();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");

		IsValid = false;
	}

	/// <summary>
	/// Disposes of all sound instances and releases the underlying buffer and system resources.
	/// </summary>
	public void Dispose()
	{
		SoundInstancePool.OnSoundDispose(this);

		Buffer?.Dispose();

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");

		GC.SuppressFinalize(this);

		IsValid = false;
	}

	/// <summary>
	/// Loads the sound buffer into memory from the file specified by <see cref="Tag"/>.
	/// </summary>
	/// <returns>
	/// The number of bytes loaded from disk into the sound buffer.
	/// </returns>
	/// <exception cref="FileNotFoundException">
	/// Thrown if the file path does not exist or is inaccessible.
	/// </exception>
	public ulong Load()
	{
		if (IsValid)
		{
			LastAccessFrame = DateTime.UtcNow;
			return 0u;
		}

		using var s = AssetManager.OpenStream(Tag);
		using var ms = new MemoryStream();
		s.CopyTo(ms);

		var soundBytes = ms.ToArray();

		Buffer = new SFSoundBuffer(ms);

		IsValid = true;
		LastAccessFrame = DateTime.UtcNow;
		Length = (ulong)Buffer.SampleRate * 2UL;

		return Length;
	}

	/// <summary>
	/// Determines whether this sound is equal to another by comparing ID and tag.
	/// </summary>
	/// <param name="other">The other sound to compare against.</param>
	/// <returns><c>true</c> if they share the same ID and tag; otherwise, <c>false</c>.</returns>
	public bool Equals(Sound other) =>
		other != null && Id.Equals(other.Id) && Tag.Equals(other.Tag);

	/// <inheritdoc/>
	public override bool Equals(object obj) => obj is Sound value && Equals(value);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Id, Tag);

	/// <summary>
	/// Returns a formatted string representation of the sound asset, including its ID, file name, and loop flag.
	/// </summary>
	/// <returns>A short string describing the asset.</returns>
	public override string ToString() => $"Sound({Id}, {Path.GetFileName(Tag)}, {IsLooped})";
}
