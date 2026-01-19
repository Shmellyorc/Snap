namespace Snap.Engine.Sounds;

/// <summary>
/// Represents the playback state of a sound instance.
/// </summary>
public enum SoundStatus
{
	/// <summary>
	/// The sound has been stopped and is not currently playing.
	/// </summary>
	Stopped,

	/// <summary>
	/// The sound is paused and can be resumed from its current position.
	/// </summary>
	Paused,

	/// <summary>
	/// The sound is actively playing.
	/// </summary>
	Playing
}

/// <summary>
/// Represents a single instance of a <see cref="Sound"/> being played.
/// </summary>
/// <remarks>
/// A <see cref="SoundInstance"/> encapsulates the playback state and properties of an individual sound,
/// including pan, volume, pitch, and status.  
/// It provides methods to control playback (e.g., play, pause, stop) and exposes properties for
/// querying or adjusting the sound while it is active.  
/// 
/// This class implements <see cref="IDisposable"/> to ensure that unmanaged audio resources are
/// released when the instance is no longer needed.
/// </remarks>
public class SoundInstance : IDisposable
{
	#region Fields
	private readonly uint _soundId;
	private SFSound _sfSound;
	private float _volume, _pitch, _pan;
	private bool _isDisposed;
	private CancellationTokenSource _monitorCts;
	#endregion


	#region Events
	public event Action<SoundInstance> OnPlaybackFinished;
	public event Action<SoundInstance> OnDisposed;

	public TimeSpan PlayingOffset
	{
		get => IsValid ? _sfSound.PlayingOffset.ToTimeSpan() : TimeSpan.Zero;
		set
		{
			if (IsValid)
				_sfSound.PlayingOffset = SFTime.FromSeconds((float)value.TotalSeconds);
		}
	}

	private async void MonitorPlayback()
	{
		if (Status != SoundStatus.Playing)
			return;

		_monitorCts?.Cancel();
		_monitorCts = new CancellationTokenSource();

		try
		{
			while (Status == SoundStatus.Playing
			&& !_monitorCts.Token.IsCancellationRequested)
			{
				await Task.Delay(50, _monitorCts.Token);
			}

			if (!_monitorCts.Token.IsCancellationRequested)
			{
				OnPlaybackFinished?.Invoke(this);
			}
		}
		catch (TaskCanceledException) { }
	}
	#endregion




	#region Properties
	public Sound Sound { get; private set; }

	public DateTime LastUsedTime { get; internal set; }

	/// <summary>
	/// Gets the unique identifier assigned to this sound instance.
	/// </summary>
	/// <remarks>
	/// The identifier is set internally and exposed as a read-only property.  
	/// It can be used to distinguish between multiple sound instances in the system.
	/// </remarks>
	public uint Id { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the sound is currently playing.
	/// </summary>
	/// <remarks>
	/// This property evaluates to <c>true</c> if <see cref="Status"/> is not <see cref="SoundStatus.Stopped"/>.  
	/// It provides a quick way to check if the sound is active.
	/// </remarks>
	public bool IsPlaying => Status != SoundStatus.Stopped;

	/// <summary>
	/// Gets a value indicating whether this sound instance is valid.
	/// </summary>
	/// <remarks>
	/// A sound is considered valid if the underlying <c>_sound</c> reference is not <c>null</c>,  
	/// is not marked as invalid, and its status is not <c>SFSoundStatus.Stopped</c>.  
	/// This property ensures that only usable sound instances are treated as valid.
	/// </remarks>
	public bool IsValid => _sfSound != null && !_sfSound.IsInvalid;

	/// <summary>
	/// Gets the current playback status of this sound instance.
	/// </summary>
	/// <remarks>
	/// If the sound is valid, this property maps the underlying <c>SFSoundStatus</c> to <see cref="SoundStatus"/>.  
	/// If the sound is invalid, the status defaults to <see cref="SoundStatus.Stopped"/>.
	/// </remarks>
	public SoundStatus Status => IsValid ? (SoundStatus)_sfSound.Status : SoundStatus.Stopped;

	/// <summary>
	/// Gets or sets the stereo pan value for this sound instance.
	/// </summary>
	/// <remarks>
	/// The value is clamped between -1.0 (full left) and 1.0 (full right).  
	/// When set, if the sound is valid, the underlying audio engine properties are updated:  
	/// <list type="bullet">
	/// <item>
	/// <description><c>RelativeToListener</c> is enabled and attenuation/min distance are set to 0 when panning left or right.</description>
	/// </item>
	/// <item>
	/// <description><c>RelativeToListener</c> is disabled and attenuation/min distance are set to 1 when centered.</description>
	/// </item>
	/// </list>
	/// The sound’s <c>Position</c> is updated to reflect the pan value on the X axis.
	/// </remarks>
	/// <value>
	/// A <see cref="float"/> representing the stereo pan, clamped between -1.0 and 1.0.
	/// </value>
	public float Pan
	{
		get => _pan;
		set
		{
			if (_pan == value)
				return;
			_pan = Math.Clamp(value, -1f, 1f);

			if (IsValid)
			{
				if (_pan < 0f || _pan > 0f)
				{
					_sfSound.RelativeToListener = true;
					_sfSound.Attenuation = 0f;
					_sfSound.MinDistance = 0f;
				}
				else
				{
					_sfSound.RelativeToListener = false;
					_sfSound.Attenuation = 1f;
					_sfSound.MinDistance = 1f;
				}

				_sfSound.Position = new(_pan, 0, 0);
			}
		}
	}

	/// <summary>
	/// Gets or sets the volume level for this sound instance.
	/// </summary>
	/// <remarks>
	/// The value is clamped between 0.0 (silent) and 1.0 (full volume).  
	/// When set, if the sound is valid, the underlying engine’s <c>Volume</c> property is updated by scaling
	/// the normalized value to a percentage range (0–100).  
	/// This ensures external consumers work with a normalized float while the engine receives its expected scale.
	/// </remarks>
	/// <value>
	/// A <see cref="float"/> representing the normalized volume level, clamped between 0.0 and 1.0.
	/// </value>
	public float Volume
	{
		get => _volume;
		set
		{
			if (_volume == value)
				return;
			_volume = Math.Clamp(value, 0f, 1f);

			if (IsValid)
				_sfSound.Volume = Math.Clamp(_volume * 100f, 0f, 100f);
		}
	}

	/// <summary>
	/// Gets or sets the pitch adjustment for this sound instance.
	/// </summary>
	/// <remarks>
	/// The value is clamped between -3.0 and 3.0, where 0.0 represents the original pitch.  
	/// Negative values lower the pitch, positive values raise it.  
	/// When set, if the sound is valid, the underlying engine’s <c>Pitch</c> property is updated directly.  
	/// This allows consumers to work with a normalized float range while the engine applies the actual pitch shift.
	/// </remarks>
	/// <value>
	/// A <see cref="float"/> representing the pitch adjustment, clamped between -3.0 and 3.0.
	/// </value>
	public float Pitch
	{
		get => _pitch;
		set
		{
			if (_pitch == value)
				return;
			_pitch = Math.Clamp(value, -3f, 3f);

			if (IsValid)
				_sfSound.Pitch = _pitch;
		}
	}
	#endregion


	#region Constructor / Deconstructor
	internal SoundInstance(uint id, Sound sound, SFSoundBuffer buffer)
	{
		Id = id;
		_soundId = sound.Id;
		Sound = sound;

		_sfSound = new SFSound(buffer)
		{
			// set volume to zero so it doesnt make any pops/weird 
			// sounds when it is intialized
			Volume = 0f,
			Loop = sound.IsLooped
		};
	}

	/// <summary>
	/// Finalizes the <see cref="SoundInstance"/> by releasing unmanaged resources.
	/// </summary>
	/// <remarks>
	/// This destructor calls <see cref="Dispose()"/> to ensure that unmanaged resources are released
	/// when the instance is garbage collected.  
	/// Consumers should still call <see cref="Dispose()"/> explicitly to deterministically free resources,
	/// rather than relying on the finalizer.
	/// </remarks>
	~SoundInstance() => Dispose();
	#endregion


	#region Play, Pause, Resume, Stop
	/// <summary>
	/// Begins playback of the sound instance.
	/// </summary>
	/// <remarks>
	/// If the sound is already playing, this method does nothing.  
	/// If the underlying sound object is invalid, a new instance is created from its <c>SoundBuffer</c>.  
	/// The sound’s <c>Volume</c>, <c>Position</c>, and <c>Pitch</c> are updated based on the current
	/// property values before playback begins.
	/// </remarks>
	public void Play()
	{
		if (IsPlaying)
			return;

		if (_sfSound.IsInvalid)
			_sfSound = new(Sound.Buffer);

		_sfSound.Volume = Math.Clamp(_volume * 100f, 0f, 100f);
		_sfSound.Position = new(_pan, 0, 0);
		_sfSound.Pitch = _pitch;
		_sfSound.Play();

		MonitorPlayback();
	}

	/// <summary>
	/// Pauses playback of the sound instance.
	/// </summary>
	/// <remarks>
	/// This method only pauses the sound if it is valid and currently playing.  
	/// If the sound is stopped or invalid, no action is taken.
	/// </remarks>
	public void Pause()
	{
		if (!IsValid || Status != SoundStatus.Playing) return;
		_sfSound.Pause();
	}

	/// <summary>
	/// Resumes playback of the sound instance if it is currently paused.
	/// </summary>
	/// <remarks>
	/// This method only resumes the sound if it is valid and its <see cref="Status"/> is <see cref="SoundStatus.Paused"/>.  
	/// If the sound is stopped or invalid, no action is taken.
	/// </remarks>
	public void Resume()
	{
		if (!IsValid || Status != SoundStatus.Paused) return;
		_sfSound.Play();
	}

	/// <summary>
	/// Stops playback of the sound instance.
	/// </summary>
	/// <remarks>
	/// This method only stops the sound if it is valid and not already stopped.  
	/// Once stopped, the sound cannot be resumed from its previous position; it must be played again
	/// from the beginning using <see cref="Play()"/>.
	/// </remarks>
	public void Stop()
	{
		if (!IsValid || Status == SoundStatus.Stopped) return;
		_sfSound.Stop();
		_monitorCts.Cancel();
	}
	#endregion


	#region IDispose
	/// <summary>
	/// Releases the unmanaged resources used by this <see cref="SoundInstance"/> and optionally disposes of managed resources.
	/// </summary>
	/// <param name="disposing">
	/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
	/// </param>
	/// <remarks>
	/// This method follows the standard .NET dispose pattern.  
	/// When <paramref name="disposing"/> is <c>true</c>, the underlying <c>_sound</c> object is disposed and a log entry
	/// is written to indicate that the asset has been unloaded.  
	/// The <c>_isDisposed</c> flag ensures that disposal occurs only once.
	/// </remarks>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			_monitorCts?.Cancel();
			_monitorCts?.Dispose();

			OnDisposed?.Invoke(this);
			SoundInstancePool.ReleaseInstance(this);

			_sfSound.Dispose();
			Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}' from Sound ID {_soundId}.");

			_isDisposed = true;
		}
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <remarks>
	/// This method calls <see cref="Dispose(bool)"/> with <c>true</c> to release both managed and unmanaged resources,
	/// and then suppresses finalization using <see cref="GC.SuppressFinalize(object)"/>.  
	/// Consumers should call this method to deterministically free resources rather than relying on the finalizer.
	/// </remarks>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion



	



	internal void Reset(Sound newSound)
	{
		if (Sound != null && Sound.Id != newSound.Id)
		{
			SoundInstancePool.OnSoundDispose(Sound);
		}

		Sound = newSound;

		_sfSound.Volume = 0f;
		_sfSound.Pitch = 1f;
		_sfSound.Position = new(0, 0, 0);
		_sfSound.RelativeToListener = false;
		_sfSound.Loop = newSound.IsLooped;
	}
}
