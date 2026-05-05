using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public float masterVolume = 1.0f;
    public string activeSlotID; 
    public List<SaveSlot> allSlots = new List<SaveSlot>();
}