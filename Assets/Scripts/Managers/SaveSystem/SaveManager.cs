using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : Singleton<SaveManager>
{
    [HideInInspector] public SaveGameShrine activeShrine;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;   // thời gian anim ẩn/hiện
    [SerializeField] private float _typewriterDuration = 1.0f; // thời gian đánh text
    [SerializeField] private float _closeDelayTime = 2f;
    
    [Header("Save UI")]
    [SerializeField] private GameObject _saveGamePanel;
    private CanvasGroup _savePanelCanvasGroup;
    [SerializeField] private Button _saveButton;

    [Header("Save data")]
    public ProfileData CurrentProfile {get; private set;}
    public SaveSnapshotData CurrentSnapshot {get; private set;}

    private void Start()
    {
        if (_saveGamePanel != null)
        {
            _saveGamePanel.SetActive(false);
        }
        if (_saveButton != null)
        {
            _saveButton.onClick.AddListener(ExcuteSave);
        }
    }

    #region Save Profile

    public void CreateNewProfile(string playerName)
    {
        CurrentProfile = new ProfileData(playerName);
        CurrentSnapshot = new SaveSnapshotData();
    }

    public void CreateNewSave(string shrineID, string sceneName)
    {
        SaveSnapshotData newSnapshot = new SaveSnapshotData();
        newSnapshot.snapshotName = $"Save + {CurrentProfile.saveHistory.Count + 1}";
        newSnapshot.reviveShrineID = shrineID;
        newSnapshot.sceneName = sceneName;
        newSnapshot.timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        newSnapshot.interactedShrinesAtThisTime = new List<string>(CurrentSnapshot.interactedShrinesAtThisTime);
        if (!newSnapshot.interactedShrinesAtThisTime.Contains(shrineID))
        {
            newSnapshot.interactedShrinesAtThisTime.Add(shrineID);
        }

        CurrentProfile.saveHistory.Add(newSnapshot);
        CurrentSnapshot = newSnapshot;

        SaveProfileToDisk();
    }

    private void SaveProfileToDisk()
    {
        string path = Path.Combine(Application.persistentDataPath, $"{CurrentProfile.profileName}_profile.json");
        string json = JsonUtility.ToJson(CurrentProfile, true);
        File.WriteAllText(path, json);
        Debug.Log($"Đã tạo File Save mới trong {path}!");
    }


    #endregion


    #region UI
    public void ShowSaveOption(bool open, bool waiting = true)
    {
        // Lấy CanvasGroup nếu chưa có
        if (_savePanelCanvasGroup == null)
            _savePanelCanvasGroup = _saveGamePanel.GetComponent<CanvasGroup>();

        RectTransform panelRect = _saveGamePanel.GetComponent<RectTransform>();

        // Dừng các Tween cũ đang chạy trên panel này để tránh xung đột (như bấm nút liên tục)
        panelRect.DOKill();
        _savePanelCanvasGroup.DOKill();

        if (open)
        {
            // Reset trạng thái trước khi hiện
            _saveGamePanel.SetActive(true);
            panelRect.localScale = Vector3.zero;
            _savePanelCanvasGroup.alpha = 0f;

            // Diễn hoạt hiện lên: Phóng to + Rõ dần
            panelRect.DOScale(Vector3.one, _animationDuration)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true);
            
            _savePanelCanvasGroup.DOFade(1f, _animationDuration)
                                 .SetUpdate(true);
        }
        else
        {
            // Diễn hoạt ẩn đi: Thu nhỏ + Mờ dần
            panelRect.DOScale(Vector3.zero, _animationDuration)
                     .SetEase(Ease.InBack)
                     .SetUpdate(true)
                     .SetDelay(waiting ?_closeDelayTime : 0);

            _savePanelCanvasGroup.DOFade(0f, _animationDuration)
                                 .SetUpdate(true)
                                 .SetDelay(_closeDelayTime)
                                 .OnComplete(() => _saveGamePanel.SetActive(false)); // Tắt Object khi ẩn xong
        }
    }
    
    public void ExcuteSave()
    {
        CreateNewSave("sample_ID", "sample_scene");
        ShowSaveOption(false, false);
    }

    
    #endregion
}
