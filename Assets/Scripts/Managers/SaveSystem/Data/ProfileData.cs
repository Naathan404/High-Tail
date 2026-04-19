using System.Collections.Generic;

[System.Serializable]
public class ProfileData
{
    public string profileName; 
    public SettingData settings; // Cài đặt âm thanh, đồ họa của Hưng
    
    // ĐÂY LÀ ĐIỀU ÔNG MUỐN: 1 List chứa rất nhiều lần lưu!
    public List<SaveSnapshotData> saveHistory; 

    public ProfileData(string name)
    {
        profileName = name;
        settings = new SettingData();
        saveHistory = new List<SaveSnapshotData>();
    }
}