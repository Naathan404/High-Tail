using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    [Header("Position")]
    public Vector3 position = Vector3.zero;
    public string currentScene;
    public string lastCheckpointID = "";

    [Header("Health and Energy")]
    public int hp = 100;
    public int energy = 100;

    [Header("Skills")]
    public bool WallJumpUnlocked = false;
    public bool WallSlideUnlocked = false;
    public bool DashUnlocked = false;
    public bool AirGlideUnlocked = false;
}
