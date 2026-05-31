using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _lastTime;
    [SerializeField] private TextMeshProUGUI _spendTime;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _deleteButton;

    private string _slotID;

    public void Setup(SaveSlot slot)
    {
        _slotID = slot.saveID;
        
        _nameText.text = slot.saveName;
        _lastTime.text = slot.lastSaveTimestamp;
        _spendTime.text = GetFormattedPlayTime(slot.totalPlayTimeSeconds);


        _loadButton.onClick.RemoveAllListeners();
        _loadButton.onClick.AddListener(OnLoadClicked);

        _deleteButton.onClick.RemoveAllListeners();
        _deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnLoadClicked()
    {
        SaveManager.Instance.LoadGameFromSlot(_slotID);
        MenuManager.Instance.ClosePauseMenu();
    }

    private void OnDeleteClicked()
    {
        SaveManager.Instance.DeleteSaveNode(_slotID);
    }

    public string GetFormattedPlayTime(float totalSeconds)
    {
        int hours = Mathf.FloorToInt(totalSeconds / 3600f);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);

        // Trả về chuỗi dạng 01:25:09
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}