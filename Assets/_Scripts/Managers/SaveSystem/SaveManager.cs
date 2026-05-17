using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : Singleton<SaveManager>
{
    [Header("Default Save Setting")]
    [HideInInspector] public SaveGameShrine activeShrine;
    public GameData MainData { get; private set; }
    public int MaxSaveSlots { get; private set; } = 3;
    [SerializeField] private int _currentSaveSlotCount = 0;
    private string _savePath;
    [SerializeField] private PlayerController _player;

    [Header("New Game Setting")]
    [SerializeField] private string _startSceneName;
    [SerializeField] private string _menuBackgroundScene; // Màn hình cảnh vật lúc ở Menu
    [SerializeField] private Vector3 _startPosition;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _closeDelayTime = 2f;
    [SerializeField] private float _notificationDuration = 2f;
    private DG.Tweening.Sequence _panelSequence;
    private RectTransform _savePanelRect;

    [Header("Save UI")]
    [SerializeField] private GameObject _saveGamePanel;
    private CanvasGroup _savePanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI _notifyText;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup _transitionCanvasGroup;
    [SerializeField] private float _transitionDuration = 0.5f;
    private bool _isLoading = false;
    private string _currentLoadedScene = "";

    [Header("Menu")]
    [SerializeField] private SaveMenuUI _saveMenuUI;

    public override void Awake()
    {
        base.Awake();
        _savePath = Path.Combine(Application.persistentDataPath, "MainData.json");
        Debug.Log($"Game lưu tại: {_savePath}");

        LoadMainData();
    }

    private void Start()
    {
        if (_saveGamePanel != null)
        {
            _saveGamePanel.SetActive(false);
        }
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.alpha = 0f;
            _transitionCanvasGroup.blocksRaycasts = false;
        }
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
        }
    }

    public SaveSlot GetActiveSlot()
    {
        return MainData.allSlots.Find(s => s.saveID == MainData.activeSlotID);
    }

    #region Save logic
    #region Load game
    private void LoadMainData()
    {
        bool loadedSuccessfully = false;

        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);

                if (!string.IsNullOrEmpty(json))
                {
                    MainData = JsonUtility.FromJson<GameData>(json);

                    if (MainData != null)
                    {
                        if (MainData.allSlots == null)
                            MainData.allSlots = new List<SaveSlot>();
                        if (!string.IsNullOrEmpty(MainData.activeSlotID))
                            MainData.activeSlotID = "";

                        _currentSaveSlotCount = MainData.allSlots.Count;
                        loadedSuccessfully = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi đọc file JSON: {e.Message}");
            }
        }
        if (!loadedSuccessfully)
        {
            Debug.LogWarning("Dữ liệu save không hợp lệ hoặc trống. Đang khởi tạo mới...");
            MainData = new GameData();
            if (MainData.allSlots == null)
                MainData.allSlots = new List<SaveSlot>();
        }
        // if (_currentSaveSlotCount == 0)
        // {
        //     MenuManager.Instance.OpenSubPanel(MenuManager.PanelType.Play);
        // }
    }
    #endregion

    #region New timeline
    public void CreateNewGame(string saveName = "")
    {
        if (_currentSaveSlotCount >= MaxSaveSlots)
        {
            ShowOnMenuNotification("Maximum save slots reached!");
            return;
        }

        // 1. Tạo ID riêng để dùng chung cho mọi thiết lập phía dưới
        string newSlotID = IDGenerator.GenerateUniqueID("slot");

        SaveSlot newSlot = new SaveSlot
        {
            saveID = newSlotID,
            saveName = string.IsNullOrEmpty(saveName) ? $"Save {_currentSaveSlotCount + 1}" : saveName,
            firstSaveTimestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            lastSaveTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            lastShrineID = "",
            sceneName = _startSceneName, // Màn chơi đầu tiên sẽ được load
            unlockedSkills = new SkillSaveData()
        };

        MainData.allSlots.Add(newSlot);
        _currentSaveSlotCount++;

        // 2. Gán activeSlotID thành cái save vừa tạo (KHÔNG để rỗng "" nữa)
        MainData.activeSlotID = newSlotID;
        SaveToDisk();

        // 3. Cập nhật lại UI Slot
        if (_saveMenuUI == null) _saveMenuUI = FindAnyObjectByType<SaveMenuUI>();
        if (_saveMenuUI != null) _saveMenuUI.RefreshSaveSlotContainer();

        // 4. TỰ ĐỘNG GỌI HÀM LOAD SCENE ĐỂ BẮT ĐẦU CHƠI NGAY LẬP TỨC
        LoadGameFromSlot(newSlotID);
    }
    #endregion

    #region Load slot
    public bool LoadGameFromSlot(string targetSlotID)
    {
        if (_isLoading) return false;

        SaveSlot targetNode = MainData.allSlots.Find(n => n.saveID == targetSlotID);
        if (targetNode != null)
        {
            MainData.activeSlotID = targetSlotID;
            StartCoroutine(LoadSceneRoutine(targetNode));
            MainData.activeSlotID = targetSlotID;
            SaveToDisk();
            ShowOnGameNotification($"Changed to  {targetNode.saveName}");
            return true;
        }
        ShowOnGameNotification("Load failed: Node not found!");
        return false;
    }

    private IEnumerator LoadSceneRoutine(SaveSlot node)
    {
        _isLoading = true;

        // --- 1. KÉO RÈM (MÀN HÌNH TỐI DẦN) ---
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.gameObject.SetActive(true);
            _transitionCanvasGroup.blocksRaycasts = true;
            _transitionCanvasGroup.DOFade(1f, _transitionDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_transitionDuration);
        }

        // ==========================================
        // TỪ ĐÂY TRỞ ĐI MÀN HÌNH ĐANG ĐEN HOÀN TOÀN
        // ==========================================

        // 2. Ẩn UI Menu đi (Vẫn giữ trạng thái Pause)
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.HideMenuForTransition();
        }

        // 3. Xóa Scene cũ và Load Scene mới
        if (string.IsNullOrEmpty(_currentLoadedScene))
        {
            _currentLoadedScene = _menuBackgroundScene;
        }

        if (_currentLoadedScene != node.sceneName)
        {
            Scene oldScene = SceneManager.GetSceneByName(_currentLoadedScene);
            if (oldScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_currentLoadedScene);
            }

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(node.sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
        }

        Scene targetScene = SceneManager.GetSceneByName(node.sceneName);
        if (targetScene.isLoaded)
        {
            SceneManager.SetActiveScene(targetScene);
        }
        _currentLoadedScene = node.sceneName;

        // 4. Phục hồi State (Gán vị trí nhân vật, lúc này Player vẫn đang bị ẩn)
        RestoreGameState(node);

        // 5. Bật Player và HUD lên NGAY LÚC MÀN HÌNH CÒN ĐEN
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowGameplayElements();
        }

        // Quan trọng: Đợi 1 frame để Unity cập nhật Transform và CharacterController chạm đất
        yield return null;

        // ==========================================
        // BẮT ĐẦU KÉO RÈM LÊN (SÁNG MÀN HÌNH)
        // ==========================================

        // 6. Sáng dần lên (Lúc này Player đã đứng sẵn trên map)
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.DOFade(0f, _transitionDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_transitionDuration);

            _transitionCanvasGroup.blocksRaycasts = false;
            _transitionCanvasGroup.gameObject.SetActive(false);
        }

        // 7. Cuối cùng, nhả Pause và cho phép người chơi điều khiển
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ClosePauseMenu();
        }

        _isLoading = false;
    }
    private void RestoreGameState(SaveSlot node)
    {
        RestoreShrinesState(node);
        RestorePlayerPosition(node);
        RestorePlayerSkills(node);
    }
    private void RestoreShrinesState(SaveSlot node)
    {
        SaveGameShrine[] shrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var shrine in shrines)
        {
            if (!string.IsNullOrEmpty(node.lastShrineID) && shrine.ID == node.lastShrineID)
            {
                shrine.DisableShrine(); // Vô hiệu hóa đền hiện tại
            }
            else
            {
                shrine.EnableShrine();  // Mở lại tất cả các đền khác
            }
        }
    }
    private void RestorePlayerSkills(SaveSlot node)
    {
        if (_player != null)
        {
            _player.Data.LoadSkillSaveData(node.unlockedSkills);
        }
    }

    private void RestorePlayerPosition(SaveSlot node)
    {
        if (_player == null) return;

        // Khóa CharacterController để nó không tự rớt xuống do trọng lực
        var charController = _player.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;

        // Set tọa độ
        if (string.IsNullOrEmpty(node.lastShrineID))
        {
            _player.transform.position = _startPosition;
        }
        else
        {
            SaveGameShrine targetShrine = null;
            SaveGameShrine[] shrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var shrine in shrines)
            {
                if (shrine.ID == node.lastShrineID)
                {
                    targetShrine = shrine;
                    break;
                }
            }

            if (targetShrine != null)
            {
                _player.transform.position = targetShrine.transform.position;
            }
            else
            {
                _player.transform.position = _startPosition;
            }
        }

        // MỞ KHÓA CharacterController TRONG COROUTINE HOẶC SAU ĐÓ
        // Bạn có thể mở khóa luôn ở đây, vì game đang Pause và Player đang bị SetActive(false), nó sẽ không rơi được.
        if (charController != null) charController.enabled = true;
    }
    #endregion

    #region commit
    public void ExcuteSave()
    {
        if (MainData == null || MainData.allSlots == null) return;
        if (activeShrine == null)
        {
            ShowOnGameNotification("Cannot save game. Please select a shrine to save.");
            return;
        }
        SaveSlot currentNode = MainData.allSlots.Find(n => n.saveID == MainData.activeSlotID);
        if (currentNode == null)
        {
            ShowOnGameNotification("Cannot find current save game file!");
            return;
        }

        currentNode.lastSaveTimestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        currentNode.lastShrineID = activeShrine.ID;
        currentNode.sceneName = SceneManager.GetActiveScene().name;
        currentNode.unlockedSkills = _player != null ? _player.Data.GetSkillSaveData() : new SkillSaveData();

        activeShrine.DisableShrine();
        activeShrine = null;

        SaveToDisk();
        RestoreShrinesState(currentNode);
        ShowOnGameNotification($"Saved successfully");
    }
    #endregion

    #region delete
    public void DeleteSaveNode(string nodeID)
    {
        if (MainData == null || MainData.allSlots == null) return;
        if (nodeID == MainData.activeSlotID)
        {
            ShowOnMenuNotification("Cannot delete current playing file!");
            return;
        }

        SaveSlot nodeToDelete = MainData.allSlots.Find(n => n.saveID == nodeID);
        if (nodeToDelete != null)
        {
            // Tìm SaveMenuUI nếu lỡ bị null
            if (_saveMenuUI == null) _saveMenuUI = FindAnyObjectByType<SaveMenuUI>();

            if (_saveMenuUI != null)
            {
                // Gọi Panel Confirm từ SaveMenuUI
                _saveMenuUI.ShowDeleteConfirmPopup(
                    $"Are you sure you want to delete\n'{nodeToDelete.saveName}'?",
                    () => ExecuteDelete(nodeToDelete) // Action truyền vào để thực thi khi bấm Yes
                );
            }
            else
            {
                // Backup an toàn: Lỡ UI lỗi không tìm thấy thì xóa thẳng luôn
                ExecuteDelete(nodeToDelete);
            }
        }
    }

    // Hàm thực thi việc xóa thực sự
    private void ExecuteDelete(SaveSlot nodeToDelete)
    {
        MainData.allSlots.Remove(nodeToDelete);
        _currentSaveSlotCount--;
        SaveToDisk();

        ShowOnMenuNotification($"Deleted: {nodeToDelete.saveName}");

        if (_saveMenuUI == null) _saveMenuUI = FindAnyObjectByType<SaveMenuUI>();
        if (_saveMenuUI != null) _saveMenuUI.RefreshSaveSlotContainer();
    }
    #endregion
    #endregion

    private void SaveToDisk()
    {
        string json = JsonUtility.ToJson(MainData, true);
        File.WriteAllText(_savePath, json);
    }

    #region Notification
    // Hàm duy nhất để hiển thị Popup. Tham số autoClose mặc định là true.
    public void ShowOnGameNotification(string message, bool autoClose = true, Action onComplete = null)
    {
        UIHelper.AnimatePopup(_saveGamePanel, message, _notifyText, autoClose, _notificationDuration, _animationDuration, onComplete);
    }

    // Gọi tắt Popup chủ động
    public void HideNotification(float delayTime = 0f)
    {
        UIHelper.HidePopup(_saveGamePanel, delayTime, _animationDuration);
    }

    public void ShowOnMenuNotification(string message)
    {
        MenuManager.Instance.ShowTitleNotification(message);
    }
    #endregion

    #region Return to Home
    public void ReturnToMainMenu()
    {
        if (_isLoading) return;
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        _isLoading = true;

        // 1. Kéo rèm đen
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.gameObject.SetActive(true);
            _transitionCanvasGroup.blocksRaycasts = true;
            _transitionCanvasGroup.DOFade(1f, _transitionDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_transitionDuration);
        }

        // ==========================================
        // MÀN HÌNH ĐANG ĐEN
        // ==========================================

        // 2. Xóa trạng thái đang chơi (Cực kỳ quan trọng để CanResumeGame() trả về False)
        MainData.activeSlotID = "";

        // 3. Ẩn ngay Player và Gameplay HUD
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.UpdateGameplayVisibility();
        }

        // 4. ĐÃ SỬA: Quét động để xóa tất cả các scene level đang bật (Bất kể Player đang ở phòng nào)
        string persistentScene = gameObject.scene.name; // Tên scene chứa Manager này

        // Vòng lặp quét ngược từ dưới lên để dọn dẹp các scene level đang mở
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            // Nếu không phải Manager và không phải Menu -> Unload nó
            if (scene.name != persistentScene && scene.name != _menuBackgroundScene && scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        // Sau khi dọn sạch phòng chơi, Load Scene Menu (nếu nó chưa được load)
        Scene menuScene = SceneManager.GetSceneByName(_menuBackgroundScene);
        if (!menuScene.isLoaded)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(_menuBackgroundScene, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
        }

        // Đặt Scene Menu làm scene Active
        Scene targetScene = SceneManager.GetSceneByName(_menuBackgroundScene);
        if (targetScene.isLoaded) SceneManager.SetActiveScene(targetScene);

        // Gán lại biến cho đồng bộ
        _currentLoadedScene = _menuBackgroundScene;

        // 5. Cập nhật UI thành Menu "Trắng" khởi đầu (Do CanResume đã thành false)
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OpenPauseMenu(instant: true);
        }

        yield return null;

        // ==========================================
        // SÁNG MÀN HÌNH LÊN
        // ==========================================

        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.DOFade(0f, _transitionDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_transitionDuration);

            _transitionCanvasGroup.blocksRaycasts = false;
            _transitionCanvasGroup.gameObject.SetActive(false);
        }

        _isLoading = false;
    }
    #endregion

}