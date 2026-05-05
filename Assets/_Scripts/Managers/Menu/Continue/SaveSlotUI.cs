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

        CalculateAndDisplaySpendTime(slot.firstSaveTimestamp, slot.lastSaveTimestamp);

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

    private void CalculateAndDisplaySpendTime(string firstTimeStr, string lastTimeStr)
    {
        string format = "dd/MM/yyyy HH:mm:ss"; 
        
        if (DateTime.TryParseExact(firstTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime firstTime) &&
            DateTime.TryParseExact(lastTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastTime))
        {
            TimeSpan timePlayed = lastTime - firstTime;
            
            // Gọi hàm định dạng chuỗi thời gian
            _spendTime.text = FormatPlayTime(timePlayed);
        }
        else
        {
            // Fallback nếu dữ liệu lỗi
            _spendTime.text = "00m 00s"; 
            Debug.LogWarning($"[SaveSlotUI] Không thể tính Playtime cho slot {_slotID}.");
        }
    }

    private string FormatPlayTime(TimeSpan time)
    {
        // Nếu số ngày lớn hơn 0 -> Hiển thị Ngày, Giờ, Phút, Giây
        if (time.Days > 0)
        {
            return $"{time.Days}d {time.Hours:D2}h {time.Minutes:D2}m {time.Seconds:D2}s";
        }
        // Nếu số ngày = 0 nhưng số giờ lớn hơn 0 -> Hiển thị Giờ, Phút, Giây
        else if (time.Hours > 0)
        {
            return $"{time.Hours:D2}h {time.Minutes:D2}m {time.Seconds:D2}s";
        }
        // Nếu cả ngày và giờ đều = 0 -> Hiển thị tối thiểu Phút và Giây
        else
        {
            return $"{time.Minutes:D2}m {time.Seconds:D2}s";
        }
    }
}