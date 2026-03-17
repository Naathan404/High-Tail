using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : class
{
    public static T Instance;

    public void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }
}
