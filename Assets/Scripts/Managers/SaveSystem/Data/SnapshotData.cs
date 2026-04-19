using System.Collections.Generic;

[System.Serializable]
public class SaveSnapshotData
{
    public string snapshotName;    // Tên file (VD: "Save 1 - Forest", "AutoSave")
    public string reviveShrineID;  // Đứng ở đâu lúc lưu
    public string sceneName;       // Màn nào
    public string timestamp;       // Lưu lúc mấy giờ
    
    // Danh sách các Shrine đã mở TÍNH ĐẾN THỜI ĐIỂM NÀY
    public List<string> interactedShrinesAtThisTime;

    public SaveSnapshotData()
    {
        interactedShrinesAtThisTime = new List<string>();
    }
}