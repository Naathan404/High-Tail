using System;
using System.Collections;
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

    [Header("Panels")]
    [SerializeField] private GameObject _savedGamePanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditPanel;
    [Header("Title Bar")]
    [SerializeField] private TextMeshProUGUI _titleText; // Kéo Text của Title vào đây
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

        // Khởi tạo ban đầu: Đảm bảo mọi Menu đều đóng và Game đang chạy
        // (Nếu đây là Scene Main Menu riêng biệt, bạn có thể đổi thành OpenMenu() nhé)
        ClosePauseMenu();
    }

    private void SetupButtonListeners()
    {
        // Gán trực tiếp vào các API công khai cho gọn code
        _pauseButton.onClick.AddListener(OpenPauseMenu);
        _backButton.onClick.AddListener(OnBackClicked);

        _playButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.SavedGame));
        _settingButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Settings));
        _creditButton.onClick.AddListener(() => OpenSubPanel(SubPanelType.Credit));

        _exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        // 1. Logic bật/tắt bằng phím (ESC)
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

        // 2. Logic Reset lựa chọn khi rê chuột
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            RectTransform panelRect = _sidePanel.GetComponent<RectTransform>();
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePosition, null))
            {
                if (_currentButtonIndex != -1)
                {
                    ResetSelection();
                }
            }
        }

        if (Keyboard.current == null) return;

        // 3. Logic điều hướng bàn phím
        if (/*PauseGameManager.IsGamePaused*/ true)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                SelectPrevious();
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                SelectNext();
            }

            if (_currentButtonIndex != -1 &&
                (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                _sidePanelButtons[_currentButtonIndex].onClick.Invoke();
            }
        }
    }

    #region Open menu
    public void OpenPauseMenu() // Mở sidepanel
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME
        SwitchTopLeftButton(isMenuOpen: true);
        CloseAllSubPanels(); // Đảm bảo các sub-panel bị tắt trước khi mở Side Menu
        OpenSideMenu(open: true, reset: true);
        ShowTitleBar(false);
    }

    public void ClosePauseMenu() //Tiếp tục game
    {
        if (!CanResumeGame())
        {
            OpenSubPanel(SubPanelType.SavedGame);
            ShowTitleNotification("Select a saved game to play");
            return;
        }

        CloseAllSubPanels(); // Tắt toàn bộ sub-panel
        ResetSelection();
        ShowTitleBar(false);

        PauseGameManager.SetPause(false); // TIẾP TỤC GAME
        SwitchTopLeftButton(isMenuOpen: false);

        if (_isSidePanelOpen) OpenSideMenu(false);
    }

    private bool CanResumeGame()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.MainData == null) return false;
        return !string.IsNullOrEmpty(SaveManager.Instance.MainData.activeSlotID);
    }

    private void OpenSideMenu(bool open, bool reset = false)
    {
        _isSidePanelOpen = open;
        UpdateSelectionUI();

        if (reset) ResetSelection();
        UIHelper.AnimateFade(_sidePanel, open);
    }
    #endregion

    #region Open sub menu

    public enum SubPanelType
    {
        Settings,
        Credit,
        SavedGame
    }

    public void OpenSubPanel(SubPanelType type) //Mở các chức năng con
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME (Trường hợp gọi từ script khác)
        SwitchTopLeftButton(isMenuOpen: true);

        if (_isSidePanelOpen) OpenSideMenu(false); // Tắt Side Menu đi
        CloseAllSubPanels(); // Đóng các sub-panel khác trước khi mở cái mới
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

    private void CloseAllSubPanels() //Close one or all panels
    {
        if (_settingsPanel.activeSelf) OpenPanel(_settingsPanel, false);
        if (_creditPanel.activeSelf) OpenPanel(_creditPanel, false);
        if (_savedGamePanel.activeSelf) OpenPanel(_savedGamePanel, false);
    }
    #endregion

    #region Click Handlers

    private void OnBackClicked()
    {
        // Đang ở Menu Con -> Tắt Menu Con, Quay về Side Menu
        if (_settingsPanel.activeSelf || _creditPanel.activeSelf || _savedGamePanel.activeSelf)
        {
            CloseAllSubPanels();
            OpenSideMenu(true);
            ShowTitleBar(false);
        }
        // Đang ở Side Menu -> Tắt sạch, quay lại chơi Game
        else if (_isSidePanelOpen)
        {
            ClosePauseMenu();
        }
    }

    private void OnExitClicked()
    {
        //TODO: Show confirmation dialog before quitting
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
        // CHỐT CHẶN: Nếu nút cần hiện đã hiển thị sẵn và đã to bằng kích thước chuẩn
        if (btnToShow.gameObject.activeSelf && btnToShow.transform.localScale.x >= 0.95f)
        {
            btnToHide.gameObject.SetActive(false); // Dọn dẹp nút kia cho chắc ăn
            return; // Bỏ qua không chạy hiệu ứng nữa
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

        if (_currentButtonIndex == -1)
        {
            _currentButtonIndex = _sidePanelButtons.Count - 1;
        }
        else
        {
            _currentButtonIndex--;
            if (_currentButtonIndex < 0)
            {
                _currentButtonIndex = _sidePanelButtons.Count - 1;
            }
        }
        UpdateSelectionUI();
    }

    private void SelectNext()
    {
        if (_sidePanelButtons == null || _sidePanelButtons.Count == 0) return;

        if (_currentButtonIndex == -1)
        {
            _currentButtonIndex = 0;
        }
        else
        {
            _currentButtonIndex++;
            if (_currentButtonIndex >= _sidePanelButtons.Count)
            {
                _currentButtonIndex = 0;
            }
        }
        UpdateSelectionUI();
    }

    private void UpdateSelectionUI()
    {
        if (_currentButtonIndex >= 0 && _currentButtonIndex < _sidePanelButtons.Count)
        {
            EventSystem.current.
                SetSelectedGameObject(_sidePanelButtons[_currentButtonIndex].gameObject);
        }
    }
    #endregion
}