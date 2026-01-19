namespace System;

public static class SoundExtentions
{
	public static SoundInstance PlayOneShot(this Sound sound, float volume = 1f, float pan = 0f, float pitch = 1f)
	{
		var instance = sound.CreateInstance();

		instance.Volume = volume;
		instance.Pan = pan;
		instance.Pitch = pitch;
		instance.Play();

		return instance;
	}


	public static SoundBank GetBank(this SoundManager manager, Enum bankId)
		=> manager.GetBank(bankId);
	
    
    public static SoundBank GetBank(this SoundManager manager, uint bankId)
		=> manager.GetBank(bankId);

	
    public static SoundInstance Play(this SoundManager manager, Enum bankId, Sound sound)
		=> manager.Play(bankId, sound);
	
    
    public static SoundInstance Play(this SoundManager manager, uint bankId, Sound sound)
		=> manager.Play(bankId, sound);


	public static void SetVolume(this SoundBank bank, float volume)
		=> bank.Volume = volume;



	public static SoundInstance AutoDispose(this SoundInstance instance)
	{
		instance.OnPlaybackFinished += (inst) => inst.Dispose();
		return instance;
	}


	public static SoundInstance WithVolume(this SoundInstance instance, float volume)
	{
		instance.Volume = volume;
		return instance;
	}


	public static SoundInstance WithPan(this SoundInstance instance, float pan)
	{
		instance.Pan = pan;
		return instance;
	}


	public static SoundInstance WithPitch(this SoundInstance instance, float pitch)
	{
		instance.Pitch = pitch;
		return instance;
	}


	public static SoundInstance PlayWith(this SoundInstance instance, float volume, float pan = 0f, float pitch = 1f)
	{
		instance.Volume = volume;
		instance.Pan = pan;
		instance.Pitch = pitch;
		instance.Play();

		return instance;
	}



	public static void PlayAndForget(this Sound sound, float volume = 1f, float pan = 0f, float pitch = 0f)
		=> sound.PlayOneShot(volume, pan, pitch).AutoDispose();


	public static void PlayAndForget(this SoundManager manager,
		uint bankId, Sound sound, float volume = 1f, float pan = 0f, float pitch = 1f)
	{
		var inst = manager.Play(bankId, sound);
		inst.Volume = volume;
		inst.Pan = pan;
		inst.Pitch = pitch;
		inst.AutoDispose();
	}


	public static List<SoundInstance> PlayAll(this IEnumerable<Sound> sounds,
	float volume = 1f, float pan = 0f, float pitch = 1f)
	{
		return sounds.Select(s => s.PlayOneShot(volume, pan, pitch)).ToList();
	}


    public static void StopAll(this SoundBank bank)
    {
        foreach(var soundPair in bank.Instances)
        {
            foreach(var instance in soundPair.Value)
				instance.Stop();
        }
    }





    public static string GetAudioStats(this SoundManager manager)
    {
		var (active, available, perSound) = SoundInstancePool.GetStats();
		return
			$"Banks: {manager.Count}, Instances: {active}/{available}, " +
			$"Playing: {manager.PlayCount}, Pool: {available} available";
	}


    public static void LogPlayingSounds(this SoundBank bank)
    {
        foreach(var soundPair in bank.Instances)
        {
			var playing = soundPair.Value.Where(x => x.IsPlaying).ToList();
            if(playing.Count > 0)
            {
				Logger.Instance.Log(LogLevel.Info,
					$"Bank {bank.Id}: Sound {soundPair.Key.Id} has {playing.Count} playing instances.");
			}
		}
    }
}
