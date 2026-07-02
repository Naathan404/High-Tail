using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
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
    [SerializeField] private SaveMenuUI _saveMenuUI;
    [SerializeField] private GameObject _saveGamePanel;
    private CanvasGroup _savePanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI _notifyText;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup _transitionCanvasGroup;
    [SerializeField] private float _transitionDuration = 0.5f;
    private bool _isLoading = false;
    private string _currentLoadedScene = "";

    [Header("Localization")]
    [SerializeField] private LocalizedString _errorDeletePlayingRef;
    [SerializeField] private LocalizedString _confirmDeleteSaveRef;
    [SerializeField] private LocalizedString _loadSuccessRef;
    [SerializeField] private LocalizedString _loadFailRef;
    [SerializeField] private LocalizedString _saveSuccessRef;
    [SerializeField] private LocalizedString _deleteSuccessRef;

    [Header("Playtime Tracking")]
    private float _sessionStartTime;


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

        if (MainData != null && MainData.settings != null && !string.IsNullOrEmpty(MainData.settings.keyBindings))
        {
            InputManager.Instance.LoadBindingOverrides(MainData.settings.keyBindings);
        }
    }

    private void OnApplicationQuit()
    {
        UpdateAndSavePlaytime();
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
                        if (MainData.settings == null)
                            MainData.settings = new SettingsData();

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
            if (MainData.allSlots == null) MainData.allSlots = new List<SaveSlot>();
            if (MainData.settings == null) MainData.settings = new SettingsData();
        }
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
        if (AudioManager.Instance.CurrentMusic != SoundName.None) AudioManager.Instance.StopMusic();

        SaveSlot targetNode = MainData.allSlots.Find(n => n.saveID == targetSlotID);
        if (targetNode != null)
        {
            MainData.activeSlotID = targetSlotID;
            _isLoading = true;

            // Ẩn UI Menu đi (Vẫn giữ trạng thái Pause)
            if (MenuManager.Instance != null) MenuManager.Instance.HideMenuForTransition();

            string sceneToUnload = string.IsNullOrEmpty(_currentLoadedScene) ? _menuBackgroundScene : _currentLoadedScene;

            // GỌI SCENE TRANSITION HANDLER
            SceneTransitionHandler.Instance.LoadSceneAsync(
                sceneToLoad: null,
                sceneToLoadAdditive: targetNode.sceneName,
                sceneToUnload: sceneToUnload,
                
                onMidpoint: () => 
                {
                    Debug.Log("Midpoint");
                    _currentLoadedScene = targetNode.sceneName;
                    RestoreGameState(targetNode);
                    if (MenuManager.Instance != null) MenuManager.Instance.ShowGameplayElements();
                },
                
                // onComplete: Chạy khi màn hình đã sáng
                onComplete: () =>
                {
                    if (MenuManager.Instance != null) MenuManager.Instance.ClosePauseMenu();
                    _isLoading = false;
                    _sessionStartTime = Time.time;
                    SaveToDisk();
                    ShowOnGameNotification(_loadSuccessRef.GetLocalizedString(targetNode.saveName));
                }
            );

            return true;
        }
        ShowOnGameNotification(_loadFailRef.GetLocalizedString());
        return false;
    }
    // public bool LoadGameFromSlot(string targetSlotID)
    // {
    //     if (_isLoading) return false;

    //     SaveSlot targetNode = MainData.allSlots.Find(n => n.saveID == targetSlotID);
    //     if (targetNode != null)
    //     {
    //         MainData.activeSlotID = targetSlotID;
    //         StartCoroutine(LoadSceneRoutine(targetNode));
    //         MainData.activeSlotID = targetSlotID;
    //         SaveToDisk();
    //         ShowOnGameNotification(_loadSuccessRef.GetLocalizedString(targetNode.saveName));
    //         return true;
    //     }
    //     ShowOnGameNotification(_loadFailRef.GetLocalizedString());
    //     return false;
    // }

    // private IEnumerator LoadSceneRoutine(SaveSlot node)
    // {
    //     _isLoading = true;

    //     // --- 1. KÉO RÈM (MÀN HÌNH TỐI DẦN) ---
    //     if (_transitionCanvasGroup != null)
    //     {
    //         _transitionCanvasGroup.gameObject.SetActive(true);
    //         _transitionCanvasGroup.blocksRaycasts = true;
    //         _transitionCanvasGroup.DOFade(1f, _transitionDuration).SetUpdate(true);
    //         yield return new WaitForSecondsRealtime(_transitionDuration);
    //     }

    //     // 2. Ẩn UI Menu đi (Vẫn giữ trạng thái Pause)
    //     if (MenuManager.Instance != null)
    //     {
    //         MenuManager.Instance.HideMenuForTransition();
    //     }

    //     // 3. Xóa Scene cũ và Load Scene mới
    //     if (string.IsNullOrEmpty(_currentLoadedScene))
    //     {
    //         _currentLoadedScene = _menuBackgroundScene;
    //     }

    //     if (_currentLoadedScene != node.sceneName)
    //     {
    //         Scene oldScene = SceneManager.GetSceneByName(_currentLoadedScene);
    //         if (oldScene.isLoaded)
    //         {
    //             yield return SceneManager.UnloadSceneAsync(_currentLoadedScene);
    //         }

    //         AsyncOperation loadOp = SceneManager.LoadSceneAsync(node.sceneName, LoadSceneMode.Additive);
    //         while (!loadOp.isDone) yield return null;
    //     }

    //     Scene targetScene = SceneManager.GetSceneByName(node.sceneName);
    //     if (targetScene.isLoaded)
    //     {
    //         SceneManager.SetActiveScene(targetScene);
    //     }
    //     _currentLoadedScene = node.sceneName;

    //     // 4. Phục hồi State (Gán vị trí nhân vật, lúc này Player vẫn đang bị ẩn)
    //     RestoreGameState(node);

    //     // 5. Bật Player và HUD lên NGAY LÚC MÀN HÌNH CÒN ĐEN
    //     if (MenuManager.Instance != null)
    //     {
    //         MenuManager.Instance.ShowGameplayElements();
    //     }

    //     // Quan trọng: Đợi 1 frame để Unity cập nhật Transform và CharacterController chạm đất
    //     yield return null;

    //     // 6. Sáng dần lên (Lúc này Player đã đứng sẵn trên map)
    //     if (_transitionCanvasGroup != null)
    //     {
    //         _transitionCanvasGroup.DOFade(0f, _transitionDuration).SetUpdate(true);
    //         yield return new WaitForSecondsRealtime(_transitionDuration);

    //         _transitionCanvasGroup.blocksRaycasts = false;
    //         _transitionCanvasGroup.gameObject.SetActive(false);
    //     }

    //     // 7. Cuối cùng, nhả Pause và cho phép người chơi điều khiển
    //     if (MenuManager.Instance != null)
    //     {
    //         MenuManager.Instance.ClosePauseMenu();
    //     }

    //     _isLoading = false;
    //     _sessionStartTime = Time.time;
    // }
    private void RestoreGameState(SaveSlot node)
    {
        Debug.Log("RestoreGameState");
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

            // for(int i = 0; i < 7; i++)
            // {
            //     if(_player.Data.WallJumpUnlocked && i == 0) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.WallSlideUnlocked && i == 1) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.AirGlideUnlocked && i == 2) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.DashUnlocked && i == 3) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.AstralPulseUnlocked && i == 4) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.PogoUnlocked && i == 5) _player.PlayerSkillUnlockStatus[i] = true;
            //     else if(_player.Data.GrabUnlocked && i == 6) _player.PlayerSkillUnlockStatus[i] = true;
            // }

            _player.UpdateSkillStatus();
        }
    }

    // private void RestorePlayerPosition(SaveSlot node)
    // {
    //     if (_player == null) return;

    //     // Khóa CharacterController để nó không tự rớt xuống do trọng lực
    //     var charController = _player.GetComponent<CharacterController>();
    //     if (charController != null) charController.enabled = false;

    //     // Set tọa độ
    //     if (string.IsNullOrEmpty(node.lastShrineID))
    //     {
    //         _player.transform.position = _startPosition;
    //     }
    //     else
    //     {
    //         SaveGameShrine targetShrine = null;
    //         SaveGameShrine[] shrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    //         foreach (var shrine in shrines)
    //         {
    //             if (shrine.ID == node.lastShrineID)
    //             {
    //                 targetShrine = shrine;
    //                 break;
    //             }
    //         }

    //         if (targetShrine != null)
    //         {
    //             _player.transform.position = targetShrine.transform.position;
    //         }
    //         else
    //         {
    //             _player.transform.position = _startPosition;
    //         }
    //     }

    //     // MỞ KHÓA CharacterController TRONG COROUTINE HOẶC SAU ĐÓ
    //     // Bạn có thể mở khóa luôn ở đây, vì game đang Pause và Player đang bị SetActive(false), nó sẽ không rơi được.
    //     if (charController != null) charController.enabled = true;
    // }

    private void RestorePlayerPosition(SaveSlot node)
    {
        Debug.Log("Set vị trí người chơi");
        if (_player == null) return;

        if(_player.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            playerController.enabled = false;
        }

        Vector3 targetPos = _startPosition; 
        
        if (!string.IsNullOrEmpty(node.lastShrineID))
        {
            SaveGameShrine[] shrines = FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var shrine in shrines)
            {
                if (shrine.ID == node.lastShrineID)
                {
                    targetPos = shrine.transform.position;
                    Debug.Log($"[SaveManager] Tìm thấy Đền: {shrine.ID}. Đang dịch chuyển...");
                    break;
                }
            }
        }

        _player.transform.position = targetPos;
        
        if(_player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.position = targetPos; // Ép Rigidbody
            rb.linearVelocity = Vector2.zero; // Xóa sổ mọi lực di chuyển còn tồn đọng
        }

        Physics.SyncTransforms();
        Physics2D.SyncTransforms();

        if (playerController != null) playerController.enabled = true;
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

        UpdateAndSavePlaytime();

        currentNode.lastSaveTimestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        currentNode.lastShrineID = activeShrine.ID;
        currentNode.sceneName = SceneManager.GetActiveScene().name;
        currentNode.unlockedSkills = _player != null ? _player.Data.GetSkillSaveData() : new SkillSaveData();

        activeShrine.DisableShrine();
        activeShrine = null;
        
        SaveToDisk();
        RestoreShrinesState(currentNode);
        ShowOnGameNotification(_saveSuccessRef.GetLocalizedString());
    }

    public void UpdateAndSavePlaytime()
    {
        if (MainData == null || string.IsNullOrEmpty(MainData.activeSlotID)) return;

        SaveSlot currentNode = MainData.allSlots.Find(n => n.saveID == MainData.activeSlotID);
        if (currentNode != null)
        {
            float timePlayedThisSession = Time.time - _sessionStartTime;
            currentNode.totalPlayTimeSeconds += timePlayedThisSession;

            _sessionStartTime = Time.time;

            SaveToDisk();
        }
    }
    #endregion

    #region delete
    public void DeleteSaveNode(string nodeID)
    {
        if (MainData == null || MainData.allSlots == null) return;
        if (nodeID == MainData.activeSlotID)
        {
            ShowOnMenuNotification(_errorDeletePlayingRef.GetLocalizedString());
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
                string confirmMessage = _confirmDeleteSaveRef.GetLocalizedString(nodeToDelete.saveName);

                _saveMenuUI.ShowDeleteConfirmPopup(
                    confirmMessage,
                    () => ExecuteDelete(nodeToDelete)
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

        ShowOnMenuNotification(_deleteSuccessRef.GetLocalizedString(nodeToDelete.saveName));

        if (_saveMenuUI == null) _saveMenuUI = FindAnyObjectByType<SaveMenuUI>();
        if (_saveMenuUI != null) _saveMenuUI.RefreshSaveSlotContainer();
    }
    #endregion

    #endregion

    public void SaveToDisk()
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
        _isLoading = true;
        Time.timeScale = 1f;
        
        UpdateAndSavePlaytime();
        MainData.activeSlotID = "";
        
        if (MenuManager.Instance != null) MenuManager.Instance.UpdateGameplayVisibility();

        // Tìm tất cả các Scene Level đang bật để Unload
        string persistentScene = gameObject.scene.name; 
        
        // Vì SceneTransitionHandler hiện tại chỉ hỗ trợ unload 1 scene bằng string,
        // để chắc ăn nhất trong logic return menu của ông, ta nên dùng name của Scene hiện tại
        string sceneToUnload = _currentLoadedScene; 

        SceneTransitionHandler.Instance.LoadSceneAsync(
            sceneToLoad: null,
            sceneToLoadAdditive: _menuBackgroundScene,
            sceneToUnload: sceneToUnload,
            
            onMidpoint: () => 
            {
                _currentLoadedScene = _menuBackgroundScene;
                ResetPlayerToMenuPosition();
                if (MenuManager.Instance != null) MenuManager.Instance.OpenPauseMenu(instant: true);
            },
            
            onComplete: () =>
            {
                _isLoading = false;
                InputManager.Instance.EnableControl();
            }
        );
    }

    private void ResetPlayerToMenuPosition()
    {
        if (_player == null) return;

        _player.enabled = false;

        if(!_player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            return;

        rb.linearVelocity = Vector2.zero;

        _player.transform.position = Vector2.zero;
        
        Physics.SyncTransforms();
        Physics2D.SyncTransforms();

        _player.enabled = true;
    }
    #endregion

}