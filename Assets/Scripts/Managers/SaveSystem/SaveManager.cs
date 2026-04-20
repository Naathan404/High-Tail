using System.Collections.Generic;
using System.IO;
using DG.Tweening;
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
    }

    public SaveNode GetActiveState()
    {
        return MainData.allCommits.Find(n => n.nodeID == MainData.activeNodeID);
    }

    #region Version Control
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
            InitializeTestRoot();
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

    //Init
    public void StartNewGame()
    {
        SaveNode rootNode = new SaveNode
        {
            nodeID = IDGenerator.GenerateUniqueID("rootnote"),
            parentNodeID = "",
            commitName = "New Game", //TODO: counting newgame
            timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            reviveShrineID = "",
            sceneName = _startSceneName,
            deadShrineIDs = new List<string>()
        };

        MainData.allCommits.Add(rootNode);
        MainData.activeNodeID = rootNode.nodeID;

        SaveToDisk();
        SceneManager.LoadScene(_startSceneName);
    }

    //Checkout
    public bool LoadGameFromNode(string targetNodeID)
    {
        SaveNode targetNode = MainData.allCommits.Find(n => n.nodeID == targetNodeID);
        if (targetNode != null)
        {
            MainData.activeNodeID = targetNodeID;
            SceneManager.LoadScene(targetNode.sceneName);
            Debug.Log($"checkout to {targetNode.commitName}");
            return true;
        }
        return false;
    }

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