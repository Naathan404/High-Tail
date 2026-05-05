using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    [Header("Navigation")]
    [SerializeField] private List<Button> _navigableButtons = new List<Button>();
    [SerializeField] private int _currentIndex = 0;
    

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

        _navigableButtons.Clear();
        _currentIndex = 0;

        foreach (SaveSlot node in history)
        {
            SaveSlotUI newSlot = Instantiate(_saveSlotPrefab, _contentContainer);
            newSlot.Setup(node);

            Button slotBtn = newSlot.GetComponentInChildren<Button>();
            if (slotBtn != null) _navigableButtons.Add(slotBtn);
        }
        if (currentSaveSlotCount < maxSaveSlotCount) 
        {
            Button newGameBtn = Instantiate(_newSavePrefab, _contentContainer);

            newGameBtn.onClick.RemoveAllListeners(); 
            newGameBtn.onClick.AddListener(HandleNewGameClicked);

            _navigableButtons.Add(newGameBtn);
        }

        SetupButtonNavigation();
    }

    private void HandleNewGameClicked()
    {
        int nextSlotIndex = SaveManager.Instance.MainData.allSlots.Count + 1;
        _nameInputField.text = $"Save {nextSlotIndex}";

        UIHelper.AnimateFade(_nameSavePanel, true);
        UIHelper.AnimateFade(_contentContainer.gameObject, false);

        _confirmNameButton.onClick.RemoveAllListeners();
        _confirmNameButton.onClick.AddListener(OnConfirmNameClicked);

        _nameInputField.onSubmit.RemoveAllListeners();
        _nameInputField.onSubmit.AddListener((text) => OnConfirmNameClicked());

        _nameInputField.Select();
        _nameInputField.ActivateInputField();
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

    #region Handle Keyboard Navigation
    private void SetupButtonNavigation()
    {
        if (_navigableButtons.Count == 0) return;

        for (int i = 0; i < _navigableButtons.Count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            // Tính toán Index để nối vòng (nếu là nút đầu thì nối với nút cuối, và ngược lại)
            int prevIndex = (i == 0) ? _navigableButtons.Count - 1 : i - 1;
            int nextIndex = (i == _navigableButtons.Count - 1) ? 0 : i + 1;

            nav.selectOnLeft = _navigableButtons[prevIndex];
            nav.selectOnRight = _navigableButtons[nextIndex];
            
            // Khóa đi lên đi xuống
            nav.selectOnUp = _navigableButtons[i];
            nav.selectOnDown = _navigableButtons[i];

            _navigableButtons[i].navigation = nav;
        }

        // Tự động focus nút đầu tiên
        _navigableButtons[0].Select();
    }
    #endregion
}