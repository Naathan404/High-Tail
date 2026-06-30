using UnityEngine;

public class StartAudio : MonoBehaviour
{
    [SerializeField] private SoundName soundName;
    void Start()
    {
        if (AudioManager.Instance.CurrentMusic != soundName)
        {
            AudioManager.Instance.PlayMusic(soundName);
        }
    }
}
