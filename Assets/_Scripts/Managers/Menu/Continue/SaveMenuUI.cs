using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class SaveMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private SaveSlotUI _saveSlotPrefab;
    [SerializeField] private Button _newSavePrefab;

    [Header("Name Save Slot UI")]
    [SerializeField] private GameObject _nameSavePanel;      
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private Button _confirmNameButton;
    

    private void OnEnable()
    {
        RefreshSaveSlotContainer();
        if (_nameSavePanel != null) 
        {
            _nameSavePanel.SetActive(false);
        }
    }

    public void RefreshSaveSlotContainer()
    {
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (SaveManager.Instance == null || SaveManager.Instance.MainData == null) return;

        List<SaveSlot> history = SaveManager.Instance.MainData.allSlots;
        int currentSaveSlotCount = history.Count;
        int maxSaveSlotCount = SaveManager.Instance.MaxSaveSlots;
        Debug.Log($"Lịch sử commit có {currentSaveSlotCount} node");

        foreach (SaveSlot node in history)
        {
            SaveSlotUI newSlot = Instantiate(_saveSlotPrefab, _contentContainer);
            newSlot.Setup(node);
        }
        if (currentSaveSlotCount < maxSaveSlotCount) 
        {
            Button newGameBtn = Instantiate(_newSavePrefab, _contentContainer);

            newGameBtn.onClick.RemoveAllListeners(); 
            newGameBtn.onClick.AddListener(HandleNewGameClicked);
        }
    }

    private void HandleNewGameClicked()
    {
        int nextSlotIndex = SaveManager.Instance.MainData.allSlots.Count + 1;
        _nameInputField.text = $"Save {nextSlotIndex}";

        UIHelper.AnimateFade(_nameSavePanel, true);
        UIHelper.AnimateFade(_contentContainer.gameObject, false);

        _confirmNameButton.onClick.RemoveAllListeners();
        _confirmNameButton.onClick.AddListener(OnConfirmNameClicked);
    }

    private void OnConfirmNameClicked()
    {
        string finalName = _nameInputField.text.Trim();

        if (string.IsNullOrEmpty(finalName))
        {
            MenuManager.Instance.ShowTitleNotification("Input a name");
            return;
        }

        UIHelper.AnimateFade(_nameSavePanel, false);
        SaveManager.Instance.CreateNewGame(finalName);
        UIHelper.AnimateFade(_contentContainer.gameObject, true);
    }
}