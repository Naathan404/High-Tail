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
    Player_Stomp,
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
    HardLanding,
    Player_Pogo_Mushroom,
    BiomeNotifier,
    OutOfEnergy,
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

    [Header("Volumn")]
    private float _currentMusicBaseVolume = 1f;

    private void Start()
    {
        SyncVolumeWithSettings();
        PlayMusic(SoundName.Music1);
    }

    public void PlayMusic(SoundName name)
    {
        if (name == SoundName.None) return;
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            _currentMusicBaseVolume = sound.volume;
            float master = GeneralSetting.Instance != null ? GeneralSetting.Instance.masterVolume : 1f;
            float bgm = GeneralSetting.Instance != null ? GeneralSetting.Instance.bgmVolume : 1f;

            musicSource.loop = true;
            musicSource.clip = sound.clips[0];
            musicSource.volume = _currentMusicBaseVolume * master * bgm;
            musicSource.pitch = sound.pitch;
            musicSource.Play();
        }
    }

    public void PlaySFX(SoundName name, bool isLoop = false) // Play one shot
    {
        if (name == SoundName.None) return;
        Sound sound = System.Array.Find(sounds, s => s.name == name);

        if (sound != null)
        {
            int index = Random.Range(0, sound.clips.Length);
            float pitch = sound.pitch + Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);

            sfxSource.pitch = pitch;
            sfxSource.loop = isLoop;

            float master = GeneralSetting.Instance != null ? GeneralSetting.Instance.masterVolume : 1f;
            float sfx = GeneralSetting.Instance != null ? GeneralSetting.Instance.sfxVolume : 1f;
            sfxSource.volume = master * sfx;

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
        if (sound == null || targetSource == null) return;

        int index = Random.Range(0, sound.clips.Length);
        float pitch = sound.pitch + Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);

        float master = GeneralSetting.Instance != null ? GeneralSetting.Instance.masterVolume : 1f;
        float sfx = GeneralSetting.Instance != null ? GeneralSetting.Instance.sfxVolume : 1f;

        targetSource.pitch = pitch;
        targetSource.loop = true;
        targetSource.volume = master * sfx;

        targetSource.PlayOneShot(sound.clips[index], sound.volume);
    }

    public void FadeOutAndStop(AudioSource source)
    {
        source.DOFade(0, musicFadeDuration).OnComplete(() => source.Stop());
    }

    #region Volume Control
    public void UpdateVolumes(float masterVol, float bgmVol, float sfxVol)
    {
        if (musicSource != null)
        {
            musicSource.volume = _currentMusicBaseVolume * masterVol * bgmVol;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVol * sfxVol;
        }

        // Phát âm thanh thay đổi volumn để người chơi cảm nhận được sự thay đổi
    }

    public void SyncVolumeWithSettings()
    {
        if (GeneralSetting.Instance != null)
        {
            UpdateVolumes(
                GeneralSetting.Instance.masterVolume,
                GeneralSetting.Instance.bgmVolume,
                GeneralSetting.Instance.sfxVolume
            );
        }
    }
    #endregion
}
