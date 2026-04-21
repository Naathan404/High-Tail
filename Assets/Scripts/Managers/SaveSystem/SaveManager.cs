using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : Singleton<SaveManager>
{
    [Header("Default Save Setting")]
    [HideInInspector] public SaveGameShrine activeShrine;
    public GameData MainData { get; private set; }
    [SerializeField] private int _maxSaveNodes = 3;
    [SerializeField] private int _currentSavedNodes = 0;

    private string _savePath;
    [Header("New Game Setting")]
    [SerializeField] private string _startSceneName;
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
    [SerializeField] private Button _saveButton;
    [SerializeField] private TextMeshProUGUI _notifyText;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup _transitionCanvasGroup;
    [SerializeField] private float _transitionDuration = 0.5f;
    private bool _isLoading = false;
    private string _currentLoadedScene = "";

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
        if (_saveButton != null)
        {
            _saveButton.onClick.RemoveAllListeners();
            _saveButton.onClick.AddListener(ExcuteSave); // Nút UI giờ sẽ gọi lệnh Commit mới
        }
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.alpha = 0f;
            _transitionCanvasGroup.blocksRaycasts = false;
        }
    }

    public SaveNode GetActiveState()
    {
        return MainData.allCommits.Find(n => n.nodeID == MainData.activeNodeID);
    }

    #region Version Control
    #region init, load
    private void LoadMainData() //TODO: start game, dont have active node (menu)
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
                        if (MainData.allCommits == null)
                            MainData.allCommits = new List<SaveNode>();
                        if (string.IsNullOrEmpty(MainData.activeNodeID))
                            MainData.activeNodeID = MainData.allCommits[0].nodeID;
                        RestoreGameState(GetActiveState());

                        _currentSavedNodes = MainData.allCommits.
                        FindAll(n => string.IsNullOrEmpty(n.parentNodeID)).Count;
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
            if (MainData.allCommits == null)
                MainData.allCommits = new List<SaveNode>();
        }
        if (MainData.allCommits.Count == 0)
        {
            StartNewGame();
        }
    }

    public void StartNewGame() //TODO: dont create new root node
    {
        int rootCount = MainData.allCommits.FindAll(n => n.parentNodeID == "").Count;
        SaveNode rootNode = new SaveNode
        {
            nodeID = IDGenerator.GenerateUniqueID("rootnote"),
            parentNodeID = "",
            commitName = $"New Game {rootCount + 1}",
            timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            reviveShrineID = "",
            sceneName = _startSceneName,
            deadShrineIDs = new List<string>()
        };

        MainData.allCommits.Add(rootNode);
        MainData.activeNodeID = rootNode.nodeID;
        SaveToDisk();
        StartCoroutine(LoadSceneRoutine(rootNode));
    }
    #endregion

    #region checkout

    public bool LoadGameFromNode(string targetNodeID)
    {
        if (_isLoading) return false; // Chống spam click

        SaveNode targetNode = MainData.allCommits.Find(n => n.nodeID == targetNodeID);
        if (targetNode != null)
        {
            MainData.activeNodeID = targetNodeID;
            StartCoroutine(LoadSceneRoutine(targetNode));
            MainData.activeNodeID = targetNodeID;
            SaveToDisk();
            ShowNotification($"Changed to  {targetNode.commitName}");
            return true;
        }
        ShowNotification("Load failed: Node not found!");
        return false;
    }

    private IEnumerator LoadSceneRoutine(SaveNode node)
    {
        _isLoading = true;

        // Fade out
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.blocksRaycasts = true;
            yield return _transitionCanvasGroup.DOFade(1f, _transitionDuration).SetUpdate(true).WaitForCompletion();
        }

        // BƯỚC 2: HỎI THĂM RAM CỦA UNITY
        Scene targetScene = SceneManager.GetSceneByName(node.sceneName);

        // NẾU SCENE CHƯA CÓ TRÊN RAM THÌ MỚI TIẾN HÀNH LOAD/UNLOAD
        if (!targetScene.isLoaded)
        {
            // UNLOAD MAP CŨ (Nếu có)
            if (!string.IsNullOrEmpty(_currentLoadedScene))
            {
                Scene oldScene = SceneManager.GetSceneByName(_currentLoadedScene);
                if (oldScene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(_currentLoadedScene);
                }
            }

            // LOAD MAP MỚI (Additive)
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(node.sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            // Cập nhật lại targetScene sau khi nạp xong
            targetScene = SceneManager.GetSceneByName(node.sceneName);
        }

        // BƯỚC 3: SET ACTIVE VÀ GHI NHỚ
        SceneManager.SetActiveScene(targetScene);
        _currentLoadedScene = node.sceneName;

        // BƯỚC 4: PHỤC HỒI NHÂN VẬT
        RestoreGameState(node);

        // BƯỚC 5: MỞ RÈM (Fade In)
        if (_transitionCanvasGroup != null)
        {
            yield return _transitionCanvasGroup.DOFade(0f, _transitionDuration).SetUpdate(true).WaitForCompletion();
            _transitionCanvasGroup.blocksRaycasts = false;
        }

        MenuManager.Instance?.OpenSideMenu(false);
        _isLoading = false;
    }

    private void RestoreGameState(SaveNode node)
    {
        Vector3 respawnPosition = Vector3.zero;
        if (string.IsNullOrEmpty(node.reviveShrineID))
        {
            respawnPosition = _startPosition;
        }
        SaveGameShrine[] shrines = Object.FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        bool isPlayerTeleported = false;

        foreach (var shrine in shrines)
        {
            // Kiểm tra xem ID của đền này có nằm trong danh sách Đền đã chết (deadShrineIDs) không
            if (node.deadShrineIDs != null && node.deadShrineIDs.Contains(shrine.ID))
            {
                shrine.DisableShrine();
            }
            else
            {
                shrine.EnableShrine();
            }

            // --- VIỆC 2: KIỂM TRA ĐỂ DỊCH CHUYỂN NHÂN VẬT ---
            if (!isPlayerTeleported && (shrine.ID == node.reviveShrineID || string.IsNullOrEmpty(node.reviveShrineID)))
            {
                if (string.IsNullOrEmpty(node.reviveShrineID))
                {
                    respawnPosition = _startPosition;
                }
                else respawnPosition = shrine.transform.position;
                if (player != null)
                {
                    // Tắt vật lý 
                    var charController = player.GetComponent<CharacterController>();
                    if (charController != null) charController.enabled = false;

                    // Dịch chuyển
                    player.transform.position = respawnPosition;

                    // Bật lại
                    if (charController != null) charController.enabled = true;

                    isPlayerTeleported = true;
                }
            }
        }

        if (!isPlayerTeleported)
        {
            Debug.LogWarning($"[SaveSystem] Không tìm thấy Đền nào có ID: {node.reviveShrineID} để dịch chuyển!");
        }
    }
    #endregion

    #region commit
    //Commit
    public void ExcuteSave() //TODO: show input save name
    //TODO: limit save node count
    {
        if (activeShrine == null)
        {
            return;
        }
        SaveNode currentNode = MainData.allCommits.Find(n => n.nodeID == MainData.activeNodeID);
        if (currentNode == null)
        {
            return;
        }

        SaveNode newCommit = new SaveNode
        {
            nodeID = IDGenerator.GenerateUniqueID("node"),
            parentNodeID = currentNode.nodeID,
            commitName = $"{activeShrine.gameObject.name}",
            timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            reviveShrineID = activeShrine.ID,
            sceneName = SceneManager.GetActiveScene().name,
            deadShrineIDs = new List<string>(currentNode.deadShrineIDs)
        };
        if (!newCommit.deadShrineIDs.Contains(activeShrine.ID))
        {
            newCommit.deadShrineIDs.Add(activeShrine.ID);
        }

        MainData.allCommits.Add(newCommit);
        MainData.activeNodeID = newCommit.nodeID;
        activeShrine.DisableShrine();
        activeShrine = null;

        SaveToDisk();
        ShowSaveOption(false, waiting: false);
        ShowNotification($"Saved successfully at {newCommit.commitName}");
    }
    #endregion

    #region delete
    //Delete
    public void DeleteSaveNode(string nodeID) //TODO fix delete, dont delete current node
    {
        if (MainData == null || MainData.allCommits == null) return;

        // 1. Tìm file save cần xóa
        SaveNode nodeToDelete = MainData.allCommits.Find(n => n.nodeID == nodeID);
        if (nodeToDelete != null)
        {
            //2. Cấm xóa trạm gốc (Root) để tránh hỏng file save
            if (nodeToDelete.parentNodeID == "")
            {
                Debug.LogWarning("Không thể xóa bản lưu Gốc (Root)!");
                return;
            }

            // 3. Xóa khỏi danh sách RAM
            MainData.allCommits.Remove(nodeToDelete);

            // 4. Nếu vô tình xóa trúng file save đang chơi dở -> Lùi về Root
            if (MainData.activeNodeID == nodeID)
            {
                MainData.activeNodeID = MainData.allCommits[0].nodeID;
            }

            // 5. GHI XUỐNG Ổ CỨNG NGAY LẬP TỨC
            SaveToDisk();
            Debug.Log($"Đã xóa thành công Node: {nodeID}");
        }
    }
    #endregion
    #endregion

    private void SaveToDisk()
    {
        string json = JsonUtility.ToJson(MainData, true);
        File.WriteAllText(_savePath, json);
    }

    #region UI Animation
    public void ShowSaveOption(bool open, bool waiting = true)
    {
        UpdatePanelState(show: open, isNotification: false, message: "", waiting: waiting);
    }

    public void ShowNotification(string message)
    {
        UpdatePanelState(show: true, isNotification: true, message: message, waiting: false);
    }

    // HÀM LÕI XỬ LÝ CHUNG DUY NHẤT
    private void UpdatePanelState(bool show, bool isNotification, string message, bool waiting)
    {
        if (_savePanelCanvasGroup == null) _savePanelCanvasGroup = _saveGamePanel.GetComponent<CanvasGroup>();
        if (_savePanelRect == null) _savePanelRect = _saveGamePanel.GetComponent<RectTransform>();

        // Dọn dẹp animation cũ để không bị giật lag nếu user thao tác liên tục
        _panelSequence?.Kill();
        _panelSequence = DOTween.Sequence().SetUpdate(true);

        if (show)
        {
            _saveGamePanel.SetActive(true);

            // 1. CHUYỂN ĐỔI TRẠNG THÁI HIỂN THỊ CON (CHILDREN)
            // Hiện nút nếu KHÔNG phải là thông báo. 
            if (_saveButton != null) _saveButton.gameObject.SetActive(!isNotification);

            // (Nếu bạn có nút Cancel/Đóng thứ 2, thêm dòng tương tự ở đây)
            // if (_cancelButton != null) _cancelButton.gameObject.SetActive(!isNotification);

            // Hiện Text nếu LÀ thông báo
            if (_notifyText != null)
            {
                _notifyText.gameObject.SetActive(isNotification);
                if (isNotification) _notifyText.text = message;
            }

            // 2. ANIMATION MỞ PANEL LÊN 
            // (Chỉ scale lên nếu panel đang tàng hình, nếu đang bật sẵn nút rồi thì bỏ qua để tránh giật hình)
            if (_savePanelCanvasGroup.alpha < 1f || _savePanelRect.localScale.x < 1f)
            {
                _savePanelRect.localScale = Vector3.zero;
                _savePanelCanvasGroup.alpha = 0f;

                _panelSequence.Join(_savePanelRect.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack))
                              .Join(_savePanelCanvasGroup.DOFade(1f, _animationDuration));
            }

            // 3. XỬ LÝ AUTO-CLOSE (Chỉ dành cho thông báo)
            if (isNotification)
            {
                // Chờ một lúc rồi tự động thu nhỏ + mờ dần và tắt Panel
                _panelSequence.AppendInterval(_notificationDuration)
                              .Append(_savePanelRect.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack))
                              .Join(_savePanelCanvasGroup.DOFade(0f, _animationDuration))
                              .OnComplete(() => _saveGamePanel.SetActive(false));
            }
        }
        else
        {
            // 4. CHỦ ĐỘNG ĐÓNG PANEL (Khi gọi ShowSaveOption(false))
            float delayTime = waiting ? _closeDelayTime : 0f;

            if (delayTime > 0)
                _panelSequence.AppendInterval(delayTime);

            _panelSequence.Append(_savePanelRect.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack))
                          .Join(_savePanelCanvasGroup.DOFade(0f, _animationDuration))
                          .OnComplete(() => _saveGamePanel.SetActive(false));
        }
    }

    #endregion
}