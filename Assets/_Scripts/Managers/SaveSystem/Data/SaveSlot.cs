using System.Collections.Generic;

[System.Serializable]
public class SaveSlot
{
    public string saveID;              
    public string saveName;
    public string firstSaveTimestamp;
    public string lastSaveTimestamp;
    public string lastShrineID;   
    public string sceneName;        
    public SkillSaveData unlockedSkills;
}