using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    [Header("Position")]
    public Vector3 position;
    public string currentScene;
    public string lastCheckpointID;

    [Header("Health and Energy")]
    public int hp;
    public int energy;

    [Header("Skills")]
    public bool WallJumpUnlocked;
    public bool WallSlideUnlocked;
    public bool DashUnlocked;
    public bool AirGlideUnlocked;
}
