using UnityEngine;

[System.Serializable]
public class SaveData
{
    [Header("Player Data")]
    public Vector3 playerPosition;
    public string currentScene;

    [Header("General settings")]
    public GeneralSetting.Language currentLanguage;
    public bool autoSave;

    public string saveState;
}
