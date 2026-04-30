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

                        _currentSavedNodes = MainData.allCommits.
                        FindAll(n => !string.IsNullOrEmpty(n.parentNodeID)).Count;
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
        StartCoroutine(LoadSceneRoutine(rootNode));
    }
    #endregion

    #region checkout

    public bool LoadGameFromNode(string targetNodeID)
    {
        if (_isLoading) return false; 

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

        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.blocksRaycasts = true;
            yield return _transitionCanvasGroup.DOFade(1f, _transitionDuration).SetUpdate(true).WaitForCompletion();
        }

        Scene targetScene = SceneManager.GetSceneByName(node.sceneName);

        if (!targetScene.isLoaded)
        {
            if (!string.IsNullOrEmpty(_currentLoadedScene))
            {
                Scene oldScene = SceneManager.GetSceneByName(_currentLoadedScene);
                if (oldScene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(_currentLoadedScene);
                }
            }

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(node.sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            targetScene = SceneManager.GetSceneByName(node.sceneName);
        }

        SceneManager.SetActiveScene(targetScene);
        _currentLoadedScene = node.sceneName;

        RestoreGameState(node);

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
        RestoreShrinesState(node);
        RestorePlayerPosition(node);
    }

    private void RestoreShrinesState(SaveNode node)
    {
        SaveGameShrine[] shrines = UnityEngine.Object.FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var shrine in shrines)
        {
            if (node.deadShrineIDs != null && node.deadShrineIDs.Contains(shrine.ID))
            {
                shrine.DisableShrine();
            }
            else
            {
                shrine.EnableShrine();
            }
        }
    }

    private void RestorePlayerPosition(SaveNode node)
    {
        Vector3 respawnPosition = _startPosition;
        SaveGameShrine[] shrines = UnityEngine.Object.FindObjectsByType<SaveGameShrine>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        bool isPlayerTeleported = false;

        foreach (var shrine in shrines)
        {
            if (!isPlayerTeleported && (shrine.ID == node.reviveShrineID || string.IsNullOrEmpty(node.reviveShrineID)))
            {
                if (string.IsNullOrEmpty(node.reviveShrineID))
                {
                    respawnPosition = _startPosition;
                }
                else 
                {
                    respawnPosition = shrine.transform.position;
                }

                if (player != null)
                {
                    var charController = player.GetComponent<CharacterController>();
                    if (charController != null) charController.enabled = false;

                    player.transform.position = respawnPosition;

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
    public void ExcuteSave() 
    {
        if (MainData == null || MainData.allCommits == null) return;
        if (_currentSavedNodes >= _maxSaveNodes)
        {
            ShowNotification($"Can only save {_maxSaveNodes} times. Please delete some old saves to create a new one.", () =>
            {
                MenuManager.Instance.OpenSubPanel(MenuManager.PanelType.Continue);
            });
            return;
        }
        if (activeShrine == null)
        {
            ShowNotification("Cannot save game. Please select a shrine to save.");
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
        _currentSavedNodes++;

        SaveToDisk();
        ShowSaveOption(false, waiting: false);
        ShowNotification($"Saved successfully at {newCommit.commitName}");
    }
    #endregion

    #region delete
    public void DeleteSaveNode(string nodeID) 
    {
        if (MainData == null || MainData.allCommits == null) return;

        SaveNode nodeToDelete = MainData.allCommits.Find(n => n.nodeID == nodeID);
        if (nodeToDelete != null)
        {
            if (nodeToDelete.parentNodeID == "")
            {
                Debug.LogWarning("Không thể xóa bản lưu Gốc (Root)!");
                return;
            }

            string nodeToStay = nodeToDelete.parentNodeID;
            MainData.allCommits.Remove(nodeToDelete);
            _currentSavedNodes--;

            if (MainData.activeNodeID == nodeID)
            {
                MainData.activeNodeID = nodeToStay;
                RestoreShrinesState(GetActiveState());
            }

            SaveToDisk();
            Debug.Log($"Đã xóa thành công Node: {nodeID}");
            _saveMenuUI.RefreshUI();
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
    
    // Dùng để hiện hoặc ẩn dòng chữ "Press ↑ to save" khi lại gần Shrine
    public void ShowSaveOption(bool open, bool waiting = true)
    {
        if (open)
        {
            DisplayMessage("Press ↑ to save", autoClose: false);
        }
        else
        {
            HideMessage(waiting ? _closeDelayTime : 0f);
        }
    }

    // Dùng để nảy lên các Popup thông báo (thành công, lỗi...) rồi tự tắt
    public void ShowNotification(string message, Action onComplete = null)
    {
        DisplayMessage(message, autoClose: true, onComplete);
    }

    private void DisplayMessage(string message, bool autoClose, Action onComplete = null)
    {
        if (_savePanelCanvasGroup == null) _savePanelCanvasGroup = _saveGamePanel.GetComponent<CanvasGroup>();
        if (_savePanelRect == null) _savePanelRect = _saveGamePanel.GetComponent<RectTransform>();

        _panelSequence?.Kill();
        _panelSequence = DOTween.Sequence().SetUpdate(true);

        _saveGamePanel.SetActive(true);
        if (_notifyText != null)
        {
            UIHelper.SetTextAnimated(_notifyText, message);
        }

        // Chỉ chạy animation phóng to nếu Panel đang bị ẩn đi
        if (_savePanelCanvasGroup.alpha < 1f || _savePanelRect.localScale.x < 1f)
        {
            _savePanelRect.localScale = Vector3.zero;
            _savePanelCanvasGroup.alpha = 0f;

            _panelSequence.Append(_savePanelRect.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack))
                          .Join(_savePanelCanvasGroup.DOFade(1f, _animationDuration));
        }

        // Tự động thu nhỏ và gọi callback sau khi xong
        if (autoClose)
        {
            _panelSequence.AppendInterval(_notificationDuration)
                          .Append(_savePanelRect.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack))
                          .Join(_savePanelCanvasGroup.DOFade(0f, _animationDuration))
                          .OnComplete(() => 
                          {
                              _saveGamePanel.SetActive(false);
                              onComplete?.Invoke();
                          });
        }
    }

    private void HideMessage(float delayTime)
    {
        _panelSequence?.Kill();
        _panelSequence = DOTween.Sequence().SetUpdate(true);

        if (delayTime > 0)
        {
            _panelSequence.AppendInterval(delayTime);
        }

        _panelSequence.Append(_savePanelRect.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack))
                      .Join(_savePanelCanvasGroup.DOFade(0f, _animationDuration))
                      .OnComplete(() => _saveGamePanel.SetActive(false));
    }
    #endregion
}