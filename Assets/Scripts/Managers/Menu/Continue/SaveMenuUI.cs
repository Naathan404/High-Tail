using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private SaveSlotUI _slotPrefab;
    [SerializeField] private GameObject _emptyDataText;

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

        if (SaveManager.Instance.MainData == null || SaveManager.Instance.MainData.allCommits == null)
        {
            _emptyDataText.SetActive(true);
            return;
        }

        List<SaveNode> history = SaveManager.Instance.MainData.
            allCommits.FindAll(node => !string.IsNullOrEmpty(node.parentNodeID));
        Debug.Log($"Lịch sử commit có {history.Count} node");
        if (history.Count == 0)
        {
            _emptyDataText.SetActive(true);
            return;
        }
        _emptyDataText.SetActive(false);
        history.Reverse();

        foreach (SaveNode node in history)
        {
            SaveSlotUI newSlot = Instantiate(_slotPrefab, _contentContainer);
            newSlot.Setup(node);
        }
    }
}