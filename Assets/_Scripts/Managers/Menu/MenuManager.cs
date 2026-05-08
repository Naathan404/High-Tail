using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [Header("Button")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private float _switchDuration = 0.25f;

    [Header("Side Panel")]
    [SerializeField] private GameObject _sidePanel;
    private CanvasGroup _sidePanelCanvasGroup;
    private float _fadeDuration = 0.5f;

    [Header("Navigation")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _creditButton;
    [SerializeField] private Button _exitButton;
    private List<Button> _sidePanelButtons = new List<Button>();
    [SerializeField] private int _currentButtonIndex = 0;
    private bool _isSidePanelOpen = false;

    // Biến lưu trữ nút cuối cùng được chọn để quay lại đúng vị trí
    private GameObject _lastSelectedButton; 

    [Header("Panels")]
    [SerializeField] private GameObject _savedGamePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditPanel;
    
    [Header("Title Bar")]
    [SerializeField] private TextMeshProUGUI _titleText; 
    [SerializeField] private GameObject _title;
    private string _currentDefaultTitle = "Pause Menu";
    private string _titleTweenId = "MenuTitleTween";

    public override void Awake()
    {
        base.Awake();
        if (_sidePanel != null)
        {
            if (!_sidePanel.TryGetComponent<CanvasGroup>(out _sidePanelCanvasGroup))
            {
                _sidePanelCanvasGroup = _sidePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        OpenPauseMenu();
    }

    private void InitializeUI()
    {
        _sidePanelButtons = new List<Button> {
            _playButton,
            _settingButton,
            _creditButton,
            _exitButton
        };

        foreach (var btn in _sidePanelButtons)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None; 
            btn.navigation = nav;
        }
    }

    private void SetupButtonListeners()
    {
        _pauseButton.onClick.AddListener(OpenPauseMenu);
        _backButton.onClick.AddListener(OnBackClicked);

        _playButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.SavedGame));
        _settingButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Settings));
        _creditButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Credit));

        _exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        // 1. Phím ESC xử lý Đóng/Mở
        if (InputManager.Instance.Inputs.UI.Menu.WasPressedThisFrame())
        {
            if (_isSidePanelOpen || _settingsPanel.activeSelf || _creditPanel.activeSelf || _savedGamePanel.activeSelf)
            {
                OnBackClicked();
            }
            else
            {
                OpenPauseMenu();
            }
        }

        if (!_isSidePanelOpen || _sidePanelButtons.Count == 0) return;

        // 2. Chuột: Giải phóng con trỏ khi rê chuột
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 5.0f)
        {
            RectTransform panelRect = _sidePanel.GetComponent<RectTransform>();
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePosition, null))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        if (Keyboard.current == null) return;

        // 3. ĐIỀU HƯỚNG THỦ CÔNG: Chỉ kích hoạt khi đang ở Menu chính
        if (_isSidePanelOpen && !(_settingsPanel.activeSelf || _creditPanel.activeSelf || _savedGamePanel.activeSelf))
        {
            // Đồng bộ Index phòng trường hợp người chơi click chuột xen ngang
            SyncIndexWithPointer();

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                SelectPrevious();
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                SelectNext();
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (_currentButtonIndex != -1)
                {
                    // Ghi nhớ lại nút vừa bấm trước khi mở SubPanel
                    _lastSelectedButton = _sidePanelButtons[_currentButtonIndex].gameObject;
                    _sidePanelButtons[_currentButtonIndex].onClick.Invoke();
                }
            }
        }
    }

    #region Open menu
    public void OpenPauseMenu() 
    {
        PauseGameManager.SetPause(true); 
        SwitchTopLeftButton(isMenuOpen: true);
        CloseAllSubPanels(); 
        ShowTitleBar(false);

        // MỞ TỪ GAME: Luôn ép chọn nút đầu tiên (Saved Game)
        _currentButtonIndex = 0;
        _lastSelectedButton = _sidePanelButtons[0].gameObject;

        OpenSideMenu(open: true);
    }

    public void ClosePauseMenu() 
    {
        if (!CanResumeGame())
        {
            OpenSubPanel(SubPanelType.SavedGame);
            ShowTitleNotification("Select a saved game to play");
            return;
        }

        CloseAllSubPanels(); 
        EventSystem.current.SetSelectedGameObject(null);
        ShowTitleBar(false);

        PauseGameManager.SetPause(false); 
        SwitchTopLeftButton(isMenuOpen: false);

        if (_isSidePanelOpen) OpenSideMenu(false);
    }

    private bool CanResumeGame()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.MainData == null) return false;
        return !string.IsNullOrEmpty(SaveManager.Instance.MainData.activeSlotID);
    }

    private void OpenSideMenu(bool open)
    {
        _isSidePanelOpen = open;
        UIHelper.AnimateFade(_sidePanel, open);

        if (open)
        {
            UpdateSelectionUI();
        }
    }
    #endregion

    #region Open sub menu

    public enum SubPanelType
    {
        Settings,
        Credit,
        SavedGame
    }

    public void OpenSubPanel(SubPanelType type) 
    {
        PauseGameManager.SetPause(true); 
        SwitchTopLeftButton(isMenuOpen: true);

        if (_isSidePanelOpen) OpenSideMenu(false); 
        CloseAllSubPanels(); 
        ShowTitleBar(true);

        switch (type)
        {
            case SubPanelType.Settings:
                OpenPanel(_settingsPanel, true);
                SetPanelTitle("Settings");
                break;
            case SubPanelType.Credit:
                OpenPanel(_creditPanel, true);
                SetPanelTitle("Credits");
                break;
            case SubPanelType.SavedGame:
                OpenPanel(_savedGamePanel, true);
                SetPanelTitle("Saved Games");
                break;
        }
    }

    private void OpenPanel(GameObject panel, bool open)
    {
        if (!panel.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        UIHelper.AnimateFade(panel, open);
    }

    private void CloseAllSubPanels() 
    {
        if (_settingsPanel.activeSelf) OpenPanel(_settingsPanel, false);
        if (_creditPanel.activeSelf) OpenPanel(_creditPanel, false);
        if (_savedGamePanel.activeSelf) OpenPanel(_savedGamePanel, false);
    }
    #endregion

    #region Click Handlers

    private void OnBackClicked()
    {
        // QUAY LẠI TỪ SUB-PANEL: Mở Menu chính và nó sẽ tự focus vào _lastSelectedButton
        if (_settingsPanel.activeSelf || _creditPanel.activeSelf || _savedGamePanel.activeSelf)
        {
            CloseAllSubPanels();
            ShowTitleBar(false);
            OpenSideMenu(true); 
        }
        else if (_isSidePanelOpen)
        {
            ClosePauseMenu();
        }
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }
    #endregion

    #region TopLeft Button
    private void SwitchTopLeftButton(bool isMenuOpen)
    {
        if (isMenuOpen)
        {
            AnimateButtonSwitch(_pauseButton, _backButton);
        }
        else
        {
            AnimateButtonSwitch(_backButton, _pauseButton);
        }
    }

    private void AnimateButtonSwitch(Button btnToHide, Button btnToShow)
    {
        if (btnToShow.gameObject.activeSelf && btnToShow.transform.localScale.x >= 0.95f)
        {
            btnToHide.gameObject.SetActive(false); 
            return; 
        }

        btnToHide.transform.DOKill();
        btnToShow.transform.DOKill();

        if (btnToHide.gameObject.activeSelf)
        {
            btnToHide.transform.DOScale(Vector3.zero, _switchDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    btnToHide.gameObject.SetActive(false);
                });
        }

        btnToShow.gameObject.SetActive(true);
        
        if (btnToShow.transform.localScale.x < 0.1f)
        {
            btnToShow.transform.localScale = Vector3.zero;
        }

        btnToShow.transform.DOScale(Vector3.one, _switchDuration + 0.1f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    #endregion

    #region Title Bar Logic
    public void ShowTitleBar(bool show)
    {
        if (_title != null)
        {
            UIHelper.AnimateFade(_title, show);
        }
    }

    public void SetPanelTitle(string title, bool animation = false)
    {
        _currentDefaultTitle = title;

        if (animation)
        {
            DOTween.Kill(_titleTweenId);

            if (_titleText != null)
            {
                UIHelper.SetTextAnimated(_titleText, _currentDefaultTitle);
            }
        }
        else
        {
            _titleText.SetText(title);
        }
    }

    public void ShowTitleNotification(string message, float displayDuration = 2f)
    {
        if (_titleText == null) return;

        DOTween.Kill(_titleTweenId);
        UIHelper.SetTextAnimated(_titleText, message);

        DOVirtual.DelayedCall(displayDuration, () =>
        {
            UIHelper.SetTextAnimated(_titleText, _currentDefaultTitle);
        }).SetId(_titleTweenId).SetUpdate(true);
    }
    #endregion

    #region Handle Keyboard Selection
    public void ResetSelection()
    {
        _currentButtonIndex = -1;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void SelectPrevious()
    {
        if (_sidePanelButtons == null || _sidePanelButtons.Count == 0) return;

        _currentButtonIndex--;
        if (_currentButtonIndex < 0)
        {
            _currentButtonIndex = _sidePanelButtons.Count - 1;
        }
        UpdateSelectionUI();
    }

    private void SelectNext()
    {
        if (_sidePanelButtons == null || _sidePanelButtons.Count == 0) return;

        _currentButtonIndex++;
        if (_currentButtonIndex >= _sidePanelButtons.Count)
        {
            _currentButtonIndex = 0;
        }
        UpdateSelectionUI();
    }

    private void UpdateSelectionUI()
    {
        if (_currentButtonIndex >= 0 && _currentButtonIndex < _sidePanelButtons.Count)
        {
            _lastSelectedButton = _sidePanelButtons[_currentButtonIndex].gameObject;
            EventSystem.current.SetSelectedGameObject(_lastSelectedButton);
        }
    }

    private void SyncIndexWithPointer()
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
        if (selectedObj != null)
        {
            int index = _sidePanelButtons.FindIndex(b => b.gameObject == selectedObj);
            if (index != -1 && index != _currentButtonIndex)
            {
                _currentButtonIndex = index;
            }
        }
    }
    #endregion
}