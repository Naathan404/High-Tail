using DG.Tweening;
using UnityEngine;

public enum AudioType
{
    Music,
    SFX
}

public enum SoundName
{
    None,
    Music1,
    Music2,
    Music3,
    Music4,
    Boss1,
    Boss2,
    Player_RunGrass,
    Player_RunRock,
    Player_JumpGrass,
    Player_JumpRock,
    Player_JumpMushroom,
    Player_LandGrass,
    Player_LandRock,
    Player_Dash,
    Player_ShockWave,
    Player_Death,
    Player_SlideDownGrass,
    Player_SlideDownRock,
    Player_Respawn,
    Reward,
    TaleStone,
    FallingRock,
    Platform_Crumbling,
    Platform_Falling,
    Door_Open,
    Text,
    SpawnPoint
}

[System.Serializable]
public class Sound
{
    public SoundName name;
    public AudioClip[] clips;
    public AudioType type;

    [Range(0, 1f)] public float volume = 1f;
    [Range(0, 1f)] public float pitch = 1f;
    [Range(0, 1f)] public float randomPitchVariation = 0f;
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sounds")]
    [SerializeField] private Sound[] sounds;

    [Header("Other Settings")]
    [SerializeField] private float musicFadeDuration = 0.5f;

    private void Start()
    {
        PlayMusic(SoundName.Music1);
    }

    public void PlayMusic(SoundName name)
    {
        if (name == SoundName.None) return;
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            musicSource.loop = true;
            musicSource.clip = sound.clips[0];
            musicSource.volume = sound.volume;
            musicSource.pitch = sound.pitch;
            musicSource.Play();
        }
    }

    public void PlaySFX(SoundName name, bool isLoop = false) // Play one shot
    {
        if (name == SoundName.None) return;
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        int index = Random.Range(0, sound.clips.Length);

        // Random pitch
        float pitch = sound.pitch + Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);
        if (sound != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.loop = isLoop;
            sfxSource.PlayOneShot(sound.clips[index], sound.volume);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void Play3DSound(SoundName name, AudioSource targetSource)
    {
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound == null) return;

        int index  = Random.Range(0, sound.clips.Length);
        float pitch = sound.pitch + Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);

        targetSource.pitch = pitch;
        targetSource.loop = true;
        targetSource.PlayOneShot(sound.clips[index], sound.volume);
    }

    public void FadeOutAndStop(AudioSource source)
    {
        source.DOFade(0, musicFadeDuration).OnComplete(() => source.Stop());
    }    
}
