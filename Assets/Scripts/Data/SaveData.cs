using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [Header("General settings")]
    public GeneralSetting.Language currentLanguage;
    public bool autoSave;
    public bool autoCheckPoint;

    public PlayerSaveData playerSaveData;
    public List<CheckpointData> checkpointDatas;
    public List<NPCData> NPCDatas;


    //Hiện tại dữ liệu chưa cấu trúc theo level,
    //danh sách checkpoint vẫn là một list chung, 
    //nhưng nếu sau này muốn mở rộng thêm dữ liệu khác thì có thể cấu trúc lại theo level hoặc theo checkpoint ID cũng được

    /**Các bước thêm dữ liệu lưu:
     * 1. Thêm dữ liệu muốn lưu vào đây, là một class [System.Serializable]
     * 2. Vào SaveSystem.cs, trong hàm SaveGame() thêm code để lấy dữ liệu và gán vào SaveData
     * 3. Trong SaveSystem.cs, trong hàm LoadGame() thêm code để lấy dữ liệu từ SaveData và gán lại cho game   * 
     * 
     */

}
