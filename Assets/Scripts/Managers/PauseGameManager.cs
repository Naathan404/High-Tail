using UnityEngine;

public class PauseGameManager : MonoBehaviour
{
    public static bool IsGamePaused
    {
        get;
        private set;
    } = false;

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
        if (pause)
        {
            Time.timeScale = 0;
        }
        else Time.timeScale = 1;
    }
}
