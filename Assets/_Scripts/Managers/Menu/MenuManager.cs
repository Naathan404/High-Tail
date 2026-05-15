using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [Header("Button")]
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
    private bool _isSubPanelOpen = false;

    // Biến lưu trữ nút cuối cùng được chọn để quay lại đúng vị trí
    private GameObject _lastSelectedButton; 

    [Header("Panels")]
    [SerializeField] private GameObject _savedGamePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditPanel;

    [Header("Localization Keys")]
    [SerializeField] private LocalizedString _playStringRef;
    [SerializeField] private LocalizedString _continueStringRef;
    [SerializeField] private LocalizedString _exitStringRef;
    [SerializeField] private LocalizedString _homeStringRef;
    [SerializeField] private LocalizedString _selectSavedGameNotificationRef;
    [SerializeField] private LocalizedString _settingsStringRef;
    [SerializeField] private LocalizedString _creditStringRef;

    [Header("Title Bar")]
    [SerializeField] private TextMeshProUGUI _titleText; 
    [SerializeField] private GameObject _title;
    private string _currentDefaultTitle = "Pause Menu";
    private string _titleTweenId = "MenuTitleTween";

    [Header("Gameplay Elements")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private GameObject _mainCanvas;

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
        UpdateGameplayVisibility();
        OpenPauseMenu(instant: true);
    }

    void Update()
    {
        // 1. Phím ESC xử lý Đóng/Mở
        if (InputManager.Instance.Inputs.UI.Menu.WasPressedThisFrame())
        {
            if (_isSidePanelOpen || _isSubPanelOpen)
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
        if (_isSidePanelOpen && !_isSubPanelOpen) // Đã sửa gọn lại
        {
            SyncIndexWithPointer();

            if (Keyboard.current.upArrowKey.wasPressedThisFrame) SelectPrevious();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame) SelectNext();

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (_currentButtonIndex != -1)
                {
                    _lastSelectedButton = _sidePanelButtons[_currentButtonIndex].gameObject;
                    _sidePanelButtons[_currentButtonIndex].onClick.Invoke();
                }
            }
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    #region Initialization
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
        _backButton.onClick.AddListener(() => OnBackClicked());

        _playButton.onClick.AddListener(() => OnPlayOrContinueClicked());
        _settingButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Settings));
        _creditButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Credit));

        _exitButton.onClick.AddListener(() => OnExitOrHomeClicked());
    }
    #endregion

    #region Localization
    private void OnLanguageChanged(Locale newLocale)
    {
        if (_isSidePanelOpen)
        {
            UpdateSidePanelButtons();
        }

        if (_isSubPanelOpen)
        {
            if (_settingsPanel.activeSelf)
                SetPanelTitle(_settingsStringRef.GetLocalizedString());
            else if (_creditPanel.activeSelf)
                SetPanelTitle(_creditStringRef.GetLocalizedString());
            else if (_savedGamePanel.activeSelf)
                SetPanelTitle(_selectSavedGameNotificationRef.GetLocalizedString());
        }
    }
    #endregion

    #region Gameplay Elements Visibility
    public void UpdateGameplayVisibility()
    {
        // Chỉ hiện Player và Canvas nếu đã có màn chơi được chọn
        bool isPlaying = CanResumeGame();

        if (_playerObject != null)
        {
            _playerObject.SetActive(isPlaying);
        }

        if (_mainCanvas != null)
        {
            _mainCanvas.SetActive(isPlaying);
        }
    }

    private void UpdateSidePanelButtons()
    {
        bool isInGame = CanResumeGame();

        // 1. Đổi Text của nút Play thành "Continue" hoặc "Play"
        TextMeshProUGUI playText = _playButton.GetComponentInChildren<TextMeshProUGUI>();
        if (playText != null)
        {
            playText.text = isInGame ? _continueStringRef.GetLocalizedString() : _playStringRef.GetLocalizedString();
        }

        // 2. Bật/Tắt nút Credit (Vào game thì ẩn đi)
        _creditButton.gameObject.SetActive(!isInGame);

        // 3. Đổi Text của nút Exit thành "Home" hoặc ngược lại
        TextMeshProUGUI exitText = _exitButton.GetComponentInChildren<TextMeshProUGUI>();
        if (exitText != null)
        {
            exitText.text = isInGame ? _homeStringRef.GetLocalizedString() : _exitStringRef.GetLocalizedString();
        }

        // 4. Làm mới danh sách điều hướng Phím (bỏ qua các nút đang bị ẩn)
        _sidePanelButtons.Clear();
        if (_playButton.gameObject.activeSelf) _sidePanelButtons.Add(_playButton);
        if (_settingButton.gameObject.activeSelf) _sidePanelButtons.Add(_settingButton);
        if (_creditButton.gameObject.activeSelf) _sidePanelButtons.Add(_creditButton);
        if (_exitButton.gameObject.activeSelf) _sidePanelButtons.Add(_exitButton);

        // 5. Chống lỗi out-of-index khi ấn phím Up/Down
        if (_currentButtonIndex >= _sidePanelButtons.Count)
        {
            _currentButtonIndex = 0;
        }
    }
    #endregion

    #region Open menu
    public void OpenPauseMenu(bool instant = false)
    {
        if (CanResumeGame())
        {
            PauseGameManager.SetPause(true);
        }
        CloseAllSubPanels();
        ShowTitleBar(false);

        _currentButtonIndex = 0;
        _lastSelectedButton = _sidePanelButtons[0].gameObject;

        OpenSideMenu(open: true);

        UpdateTopLeftButtonState(instant); // Truyền cờ instant xuống
    }

    public void ClosePauseMenu() 
    {
        if (!CanResumeGame())
        {
            OpenSubPanel(SubPanelType.SavedGame);
            ShowTitleNotification(_selectSavedGameNotificationRef.GetLocalizedString());
            return;
        }

        CloseAllSubPanels(); 
        EventSystem.current.SetSelectedGameObject(null);
        ShowTitleBar(false);

        PauseGameManager.SetPause(false); 

        if (_isSidePanelOpen) OpenSideMenu(false);

        UpdateTopLeftButtonState();

        //UpdateGameplayVisibility();
    }

    public void ShowGameplayElements()
    {
        if (_playerObject != null) _playerObject.SetActive(true);
        if (_mainCanvas != null) _mainCanvas.SetActive(true);
    }

    // Hàm này chỉ giấu giao diện Menu đi lúc màn hình đen, tuyệt đối KHÔNG nhả Pause
    public void HideMenuForTransition()
    {
        CloseAllSubPanels();
        ShowTitleBar(false);
        if (_isSidePanelOpen) OpenSideMenu(false);
        UpdateTopLeftButtonState(instant: true);

        // Ép ẩn luôn Player & Canvas trong lúc đang chuyển cảnh để chống giật lag UI
        if (_playerObject != null) _playerObject.SetActive(false);
        if (_mainCanvas != null) _mainCanvas.SetActive(false);
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
            UpdateSidePanelButtons();
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
        if (CanResumeGame())
        {
            PauseGameManager.SetPause(true);
        }

        if (_isSidePanelOpen) OpenSideMenu(false);

        CloseAllSubPanels();
        _isSubPanelOpen = true; // Thêm dòng này SAU KHI CloseAllSubPanels

        ShowTitleBar(true);

        switch (type)
        {
            case SubPanelType.Settings:
                OpenPanel(_settingsPanel, true);
                SetPanelTitle(_settingsStringRef.GetLocalizedString());
                break;
            case SubPanelType.Credit:
                OpenPanel(_creditPanel, true);
                SetPanelTitle(_creditStringRef.GetLocalizedString());
                break;
            case SubPanelType.SavedGame:
                OpenPanel(_savedGamePanel, true);
                SetPanelTitle(_selectSavedGameNotificationRef.GetLocalizedString());
                break;
        }

        UpdateTopLeftButtonState();
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
        _isSubPanelOpen = false;

        if (_settingsPanel.activeSelf) OpenPanel(_settingsPanel, false);
        if (_creditPanel.activeSelf) OpenPanel(_creditPanel, false);
        if (_savedGamePanel.activeSelf) OpenPanel(_savedGamePanel, false);
    }
    #endregion

    #region Click Handlers

    private void OnBackClicked()
    {
        // 1. Đang ở Sub-panel -> Tắt Sub-panel, lùi về Side Navigation
        if (_isSubPanelOpen)
        {
            CloseAllSubPanels();
            ShowTitleBar(false);
            OpenSideMenu(true);

            UpdateTopLeftButtonState(); // Sẽ tự động ẩn nút Back
        }
        // 2. Đang ở Side Navigation -> Muốn thoát Menu về Game
        else if (_isSidePanelOpen)
        {
            if (!CanResumeGame())
            {
                ShowTitleNotification(_selectSavedGameNotificationRef.GetLocalizedString());
            }
            else
            {
                ClosePauseMenu();
            }
        }
    }

    private void OnPlayOrContinueClicked()
    {
        if (CanResumeGame())
        {
            // ĐÃ VÀO GAME: Nút đóng vai trò là Continue
            // -> Đóng luôn Menu, nhả Pause, quay lại chơi tiếp (y chang nhấn ESC)
            ClosePauseMenu();
        }
        else
        {
            // CHƯA VÀO GAME (Ở Main Menu): Nút đóng vai trò là Play
            // -> Mở bảng chọn Save Slot
            OpenSubPanel(SubPanelType.SavedGame);
        }
    }

    private void OnExitOrHomeClicked()
    {
        if (CanResumeGame())
        {
            // Đang chơi game -> Nút có chức năng Home (Quay về Menu gốc)
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ReturnToMainMenu();
            }
        }
        else
        {
            // Chưa chơi game -> Nút có chức năng Exit (Thoát App)
            Application.Quit();
        }
    }

    #endregion

    #region TopLeft Button
    private void UpdateTopLeftButtonState(bool instant = false)
    {
        // Chỉ hiện nút Back nếu đang mở Settings, Credits, hoặc Play
        bool shouldShowBack = _isSubPanelOpen;

        if (shouldShowBack)
        {
            if (instant) ShowButtonInstantly(_backButton);
            else ShowButtonAnimate(_backButton);
        }
        else
        {
            if (instant) ForceHideButton(_backButton);
            else HideButtonAnimate(_backButton);
        }
    }

    private void ForceHideButton(Button btn)
    {
        btn.transform.DOKill();
        btn.transform.localScale = Vector3.zero;
        btn.gameObject.SetActive(false);
    }

    private void ShowButtonInstantly(Button btn)
    {
        btn.transform.DOKill();
        btn.gameObject.SetActive(true);
        btn.transform.localScale = Vector3.one;
    }

    private void ShowButtonAnimate(Button btn)
    {
        if (btn.gameObject.activeSelf && btn.transform.localScale.x >= 0.95f) return;

        btn.transform.DOKill();
        btn.gameObject.SetActive(true);
        if (btn.transform.localScale.x < 0.1f) btn.transform.localScale = Vector3.zero;

        btn.transform.DOScale(Vector3.one, _switchDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void HideButtonAnimate(Button btn)
    {
        if (!btn.gameObject.activeSelf || btn.transform.localScale.x <= 0.05f) return;

        btn.transform.DOKill();
        btn.transform.DOScale(Vector3.zero, _switchDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => btn.gameObject.SetActive(false));
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