using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : Singleton<SaveManager>
{
    [Header("Default Save Setting")]
    [HideInInspector] public SaveGameShrine activeShrine;
    public GameData MainData { get; private set; }
    private string _savePath;
    [SerializeField] private string _startSceneName;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _closeDelayTime = 2f;

    [Header("Save UI")]
    [SerializeField] private GameObject _saveGamePanel;
    private CanvasGroup _savePanelCanvasGroup;
    [SerializeField] private Button _saveButton;

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
                        if (MainData.allCommits == null)
                            MainData.allCommits = new List<SaveNode>();
                        if (string.IsNullOrEmpty(MainData.activeNodeID))
                            MainData.activeNodeID = MainData.allCommits[0].nodeID;
                        RestoreGameState(GetActiveState());
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
            //InitializeTestRoot();
            StartNewGame();
        }
    }

    private void InitializeTestRoot()
    {
        SaveNode testRootNode = new SaveNode
        {
            nodeID = IDGenerator.GenerateUniqueID("root"),
            parentNodeID = "",
            commitName = "Auto New Game (Test)",
            timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            sceneName = SceneManager.GetActiveScene().name,
            deadShrineIDs = new List<string>()
        };

        MainData.allCommits.Add(testRootNode);
        MainData.activeNodeID = testRootNode.nodeID;
        Debug.Log("Đã khởi tạo dữ liệu gốc thành công.");
    }

    public void StartNewGame()
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
        //SceneManager.LoadScene(_startSceneName);
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
            return true;
        }
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
            if (!isPlayerTeleported && shrine.ID == node.reviveShrineID)
            {
                if (player != null)
                {
                    // Tắt vật lý 
                    var charController = player.GetComponent<CharacterController>();
                    if (charController != null) charController.enabled = false;

                    // Dịch chuyển
                    player.transform.position = shrine.transform.position;

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
    public void ExcuteSave()
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
        ShowSaveOption(false, false);
        //TODO: show note
        Debug.Log($"Đã Save! Active Node mới là: {newCommit.nodeID}");
    }
    #endregion

    #region delete
    //Delete
    public void DeleteSaveNode(string nodeID) //TODO sửa lại quy tắc xóa, khi về offscene
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
        if (_savePanelCanvasGroup == null)
            _savePanelCanvasGroup = _saveGamePanel.GetComponent<CanvasGroup>();

        RectTransform panelRect = _saveGamePanel.GetComponent<RectTransform>();

        panelRect.DOKill();
        _savePanelCanvasGroup.DOKill();

        if (open)
        {
            _saveGamePanel.SetActive(true);
            panelRect.localScale = Vector3.zero;
            _savePanelCanvasGroup.alpha = 0f;

            panelRect.DOScale(Vector3.one, _animationDuration)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true);

            _savePanelCanvasGroup.DOFade(1f, _animationDuration)
                                 .SetUpdate(true);
        }
        else
        {
            panelRect.DOScale(Vector3.zero, _animationDuration)
                     .SetEase(Ease.InBack)
                     .SetUpdate(true)
                     .SetDelay(waiting ? _closeDelayTime : 0);

            _savePanelCanvasGroup.DOFade(0f, _animationDuration)
                                 .SetUpdate(true)
                                 .SetDelay(waiting ? _closeDelayTime : 0)
                                 .OnComplete(() => _saveGamePanel.SetActive(false));
        }
    }

    #endregion
}