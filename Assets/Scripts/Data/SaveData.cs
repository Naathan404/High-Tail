using NUnit.Framework;
using System.Collections.Generic;
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
    public bool autoCheckPoint;

    [Header("Checkpoint data")]
    public List<CheckpointData> checkpointDatas;
    public string lastCheckpointID;


    //Hiện tại dữ liệu chưa cấu trúc theo level,
    //danh sách checkpoint vẫn là một list chung, 
    //nhưng nếu sau này muốn mở rộng thêm dữ liệu khác thì có thể cấu trúc lại theo level hoặc theo checkpoint ID cũng được

    public string saveState;
}
