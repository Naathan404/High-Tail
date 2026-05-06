using Unity.VisualScripting;
using UnityEngine;

public enum AudioType
{
    Music,
    SFX
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioType type;

    [Range(0, 1f)] public float volume = 1f;
    [Range(0, 1f)] public float pitch = 1f;
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sounds")]
    public Sound[] sounds;

    public void PlayMusic(string name)
    {
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume;
            musicSource.pitch = sound.pitch;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name) // Play one shot
    {
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
