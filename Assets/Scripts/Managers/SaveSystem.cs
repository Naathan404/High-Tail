using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    string _saveFilePath;
    public static SaveSystem Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Init();
        LoadGame();
    }

    private void Init()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null )
        {
            saveData.playerPosition = player.transform.position;
            saveData.saveState = "Player found";
        }
        else
        {
            saveData.saveState = "Player not found";
            saveData.playerPosition = Vector3.zero; // Default position if player is not found
        }
        saveData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string data = JsonUtility.ToJson(saveData);
        File.WriteAllText(_saveFilePath, data);

        Debug.Log("Game saved at: " + _saveFilePath);
    }

    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(_saveFilePath)); //from JSON to CS (SaveData)

            //Load the saved scene
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
        }
        else
        {
            SaveGame();
        }

        Debug.Log("Game loaded from: " + _saveFilePath);
    }
    
    private void OnApplicationQuit()
    {
        SaveGame();
    }

}
