using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "High Tail/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public int CollectedItem = 0;
    public int RequiredItem = 0;

    public void CollectItem()
    {
        CollectedItem++;
    }

    public void ResetData()
    {
        CollectedItem = 0;
    }
}
