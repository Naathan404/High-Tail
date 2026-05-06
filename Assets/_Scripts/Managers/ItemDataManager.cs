using System;
using UnityEngine;

public class ItemDataManager : Singleton<ItemDataManager>
{
    public ItemData _itemData;
    public static event Action OnItemCollected;

    public override void Awake()
    {
        base.Awake();
        _itemData.ResetData();
    }

    public void CollectItem()
    {
        _itemData.CollectItem();
        // Update UI
        OnItemCollected?.Invoke();
    }
}
