using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    string _saveFilePath;
    CheckPoint[] _checkPoints;
    NPC[] _npcs;


    public static SaveSystem Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Init();
        LoadGame();
    }
    private void OnApplicationQuit()
    {
        if (GeneralSetting.Instance.autoSave) SaveGame();
    }

    private void Init()
    {
        _npcs = FindObjectsByType<NPC>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        //_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        //General settings
        saveData.currentLanguage = GeneralSetting.Instance.currentLanguage;
        saveData.autoSave = GeneralSetting.Instance.autoSave;
        saveData.autoCheckPoint = GeneralSetting.Instance.autoCheckPoint;

        //Other data
        saveData.playerSaveData = GetPlayerSaveData();
        saveData.checkpointDatas = GetCheckPointsState();
        saveData.NPCDatas = GetNPCsState();

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
            GeneralSetting.Instance.autoCheckPoint = saveData.autoCheckPoint;

            //Load player, checkpoints and NPCs data
            LoadPlayerData(saveData.playerSaveData);
            LoadCheckPointsState(saveData.checkpointDatas);
            LoadNPCsState(saveData.NPCDatas);
        }
        else
        {
            SaveGame();
        }

        Debug.Log("Game loaded from: " + _saveFilePath);
    }

    #region SaveLoad CheckPoints

    CheckPoint[] GetAllCheckPoints()
    {
        return FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
    private List<CheckpointData> GetCheckPointsState()
    {
        List<CheckpointData> checkPointsState = new List<CheckpointData>();
        CheckPoint[] _checkPoints = GetAllCheckPoints();

        foreach (CheckPoint checkpoint in _checkPoints)
        {
            CheckpointData data = new CheckpointData
            {
                CheckpointID = checkpoint.CheckpointID,
                IsInteracted = checkpoint.IsInteracted
            };
            checkPointsState.Add(data);
        }

        return checkPointsState;
    }

    private void LoadCheckPointsState(List<CheckpointData> data)
    {
        CheckPoint[] _checkPoints = GetAllCheckPoints();
        foreach (CheckpointData checkpointData in data)
        {
            CheckPoint checkpoint = _checkPoints.FirstOrDefault(cp => cp.CheckpointID == checkpointData.CheckpointID);
            if (checkpoint != null)
            {
                checkpoint.SetInteracted(checkpointData.IsInteracted);
            }
        }
    }
    #endregion

    #region Saveload Player
    private PlayerSaveData GetPlayerSaveData()
    {
        PlayerSaveData playerData = new PlayerSaveData();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerData.position = player.transform.position;
            playerData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (player.TryGetComponent(out PlayerController playerController))
            {
                playerData.lastCheckpointID = playerController.lastCheckPoint?.CheckpointID ?? "";
                playerData.hp = playerController.CurrentHP;
                playerData.energy = playerController.CurrentEnergy;
                playerData.WallJumpUnlocked = playerController.WallJumpUnlocked;
                playerData.WallSlideUnlocked = playerController.WallSlideUnlocked;
                playerData.DashUnlocked = playerController.DashUnlocked;
                playerData.AirGlideUnlocked = playerController.AirGlideUnlocked;
            }
            else
            {
                Debug.Log("Player Controller not found");
            }
        }
        else
        {
            Debug.Log("Player not found");
        }
        return playerData;
    }

    private void LoadPlayerData(PlayerSaveData playerData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        //Load the saved scene
        if (player != null)
        {
            player.transform.position = playerData.position;
            if (player.TryGetComponent(out PlayerController playerController))
            {
                playerController.SetHP(playerData.hp);
                playerController.SetEnergy(playerData.energy);
                playerController.lastCheckPoint = GetAllCheckPoints().FirstOrDefault(cp => cp.CheckpointID == playerData.lastCheckpointID); 
                playerController.WallJumpUnlocked = playerData.WallJumpUnlocked;
                playerController.WallSlideUnlocked = playerData.WallSlideUnlocked;
                playerController.DashUnlocked = playerData.DashUnlocked;
                playerController.AirGlideUnlocked = playerData.AirGlideUnlocked;
            }
            else
            {
                Debug.Log("Player Controller not found");
            }
        }
        else
        {
            Debug.Log("Player not found");
        }
    }
    #endregion

    #region Saveload NPCs

    NPC[] GetAllNPCs()
    {
        return FindObjectsByType<NPC>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
    private List<NPCData> GetNPCsState()
    {
        List<NPCData> npcsState = new List<NPCData>();
        _npcs = GetAllNPCs();

        foreach (NPC npc in _npcs)
        {
            NPCData data = new NPCData
            {
                NPCID = npc.NPCID,
                dialogueIndex = npc.dialogueIndex
            };
            npcsState.Add(data);
        }

        return npcsState;
    }

    private void LoadNPCsState(List<NPCData> data)
    {
        _npcs = GetAllNPCs();
        foreach (NPCData npcData in data)
        {
            NPC npc = _npcs.FirstOrDefault(n => n.NPCID == npcData.NPCID);
            if (npc != null)
            {
                npc.dialogueIndex = npcData.dialogueIndex;
            }
        }
    }
    #endregion
}