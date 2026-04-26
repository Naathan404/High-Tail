using System.Collections.Generic;

[System.Serializable]
public class SaveNode
{
    public string nodeID;          
    public string parentNodeID;    
    
    public string commitName;  
    public string timestamp;
    
    public string reviveShrineID;   
    public string sceneName;        
    public List<string> deadShrineIDs = new List<string>(); 
}