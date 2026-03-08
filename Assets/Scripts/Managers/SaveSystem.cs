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
        
        //Player
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

        //General settings
        saveData.currentLanguage = GeneralSetting.Instance.currentLanguage;
        saveData.autoSave = GeneralSetting.Instance.autoSave;

        //Save to JSON
        string data = JsonUtility.ToJson(saveData);
        File.WriteAllText(_saveFilePath, data);

        Debug.Log("Game saved at: " + _saveFilePath);
    }

    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            //From JSON to CS (SaveData)
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(_saveFilePath)); 

            //Load general settings
            GeneralSetting.Instance.currentLanguage = saveData.currentLanguage;
            GeneralSetting.Instance.autoSave = saveData.autoSave;

            //Load player position
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
        if (GeneralSetting.Instance.autoSave) SaveGame();
    }

}
