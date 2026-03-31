using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : class
{
    public static T Instance;

    public void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
