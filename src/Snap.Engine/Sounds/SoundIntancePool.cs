using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.Engine.Sounds;

public static class SoundInstancePool
{
	private static readonly List<SoundInstance> _activeInstances = new(256);
	private static readonly Queue<SoundInstance> _availbleInstances = new(64);
	private static readonly Dictionary<uint, List<SoundInstance>> _soundToInstances = new();
	private static readonly object _lock = new();
	private static uint _nextInstanceId = 1;

	public const int MAX_GLOBAL_INSTANCES = 255;
	public const int MAX_INSTANCSE_PER_SOUND = 16;

	public static SoundInstance GetInstance(Sound sound)
	{
		lock (_lock)
		{
			if (_soundToInstances.TryGetValue(sound.Id, out var soundInstances))
			{
				if (soundInstances.Count >= MAX_INSTANCSE_PER_SOUND)
				{
					var reusable = soundInstances.FirstOrDefault(x => x.Status == Sounds.SoundStatus.Stopped);

					if (reusable != null)
					{
						reusable.Reset(sound);
						return reusable;
					}

					Logger.Instance.Log(LogLevel.Warning, $"[Audio] Sounds {sound.Id} has reached instances limit ({MAX_INSTANCSE_PER_SOUND})");

					return null;
				}
			}

			if (_availbleInstances.Count > 0)
			{
				var instance = _availbleInstances.Dequeue();
				instance.Reset(sound);
				RegisterInstance(instance, sound);
				return instance;
			}

			if (_activeInstances.Count >= MAX_GLOBAL_INSTANCES)
			{
				var oldestStopped = _activeInstances
					.Where(x => x.Status == Sounds.SoundStatus.Stopped)
					.OrderBy(x => x.LastUsedTime)
					.FirstOrDefault();

				if (oldestStopped != null)
				{
					UnregisterInstance(oldestStopped);
					oldestStopped.Reset(sound);
					RegisterInstance(oldestStopped, sound);
					return oldestStopped;
				}

				Logger.Instance.Log(LogLevel.Warning,
					$"[Audio] Global sound instance limit reached ({MAX_GLOBAL_INSTANCES})");

				return null;
			}

			var newInstance = new SoundInstance(_nextInstanceId++, sound, sound.Buffer);
			RegisterInstance(newInstance, sound);
			_activeInstances.Add(newInstance);

			return newInstance;
		}
	}


	public static void ReleaseInstance(SoundInstance instance)
	{
		lock (_lock)
		{
			if (instance == null || !instance.IsValid)
				return;

			UnregisterInstance(instance);

			instance.Stop();
			_availbleInstances.Enqueue(instance);
		}
	}

	public static void OnSoundDispose(Sound sound)
	{
		lock (_lock)
		{
			if (_soundToInstances.TryGetValue(sound.Id, out var instances))
			{
				foreach (var instance in instances.ToList())
				{
					instance.Stop();
					ReleaseInstance(instance);
				}

				_soundToInstances.Remove(sound.Id);
			}
		}
	}

	private static void RegisterInstance(SoundInstance instance, Sound sound)
	{
		instance.LastUsedTime = DateTime.UtcNow;

		if (!_soundToInstances.TryGetValue(sound.Id, out var list))
		{
			list = new List<SoundInstance>();
			_soundToInstances[sound.Id] = list;
		}

		list.Add(instance);
	}

	private static void UnregisterInstance(SoundInstance instance)
	{
		if (instance != null && instance.Sound != null &&
		_soundToInstances.TryGetValue(instance.Sound.Id, out var list))
		{
			list.Remove(instance);

			if(list.Count == 0)
				_soundToInstances.Remove(instance.Sound.Id);
		}

		_activeInstances.Remove(instance);
	}

	public static (int active, int available, Dictionary<uint, int> perSound) GetStats()
	{
		lock (_lock)
		{
			var perSoundCounts = _soundToInstances.ToDictionary(
				kv => kv.Key,
				kv => kv.Value.Count
			);

			return (_activeInstances.Count, _availbleInstances.Count, perSoundCounts);
		}
	}

	public static bool HasActiveInstances(Sound sound)
	{
		lock(_lock)
		{
			if(_soundToInstances.TryGetValue(sound.Id, out var instances))
			{
				return instances.Any(x => x.IsValid && x.Status != SoundStatus.Stopped);
			}

			return false;
		}
	}
}
