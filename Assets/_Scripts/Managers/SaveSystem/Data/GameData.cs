using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public SettingsData settings = new SettingsData();
    public string activeSlotID; 
    public List<SaveSlot> allSlots = new List<SaveSlot>();
}

[System.Serializable]
public class SettingsData
{
    public int languageIndex = 0; // 0: Vietnamese, 1: English
    public bool autoSave = true;
    public string mainCharacterName = "";
}