using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public float masterVolume = 1.0f;
    public string activeNodeID; 
    public List<SaveNode> allCommits = new List<SaveNode>();
    
}