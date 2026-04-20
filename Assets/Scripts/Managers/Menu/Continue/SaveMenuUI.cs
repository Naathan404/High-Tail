using UnityEngine;
using System.Collections.Generic;

public class SaveMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private SaveSlotUI _slotPrefab; //TODO tạo prefabs

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (SaveManager.Instance.MainData == null || SaveManager.Instance.MainData.allCommits.Count == 0)
        {
            // TODO: Hiển thị dòng chữ "Chưa có dữ liệu" trên UI
            return;
        }

        List<SaveNode> history = new List<SaveNode>(SaveManager.Instance.MainData.allCommits);
        history.Reverse();

        foreach (SaveNode node in history)
        {
            SaveSlotUI newSlot = Instantiate(_slotPrefab, _contentContainer);
            newSlot.Setup(node);
        }
    }
}