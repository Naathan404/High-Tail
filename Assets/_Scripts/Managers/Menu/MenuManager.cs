using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditPanel;
    [SerializeField] private GameObject _playPanel;

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
        CloseAllMenus();
    }

    private void SetupButtonListeners()
    {
        // Gán trực tiếp vào các API công khai cho gọn code
        _pauseButton.onClick.AddListener(OpenMenu);
        _backButton.onClick.AddListener(OnBackClicked);

        _playButton.onClick.AddListener(() => OpenSubPanel(PanelType.Play));
        _settingButton.onClick.AddListener(() => OpenSubPanel(PanelType.Settings));
        _creditButton.onClick.AddListener(() => OpenSubPanel(PanelType.Credit));

        _exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        // 1. Logic bật/tắt bằng phím (ESC)
        if (InputManager.Instance.Inputs.UI.Menu.WasPressedThisFrame())
        {
            if (_isSidePanelOpen || _settingsPanel.activeSelf || _creditPanel.activeSelf || _playPanel.activeSelf)
            {
                OnBackClicked();
            }
            else
            {
                OpenMenu();
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

    #region Public API (LUỒNG CHÍNH ĐỂ QUẢN LÝ PAUSE/UNPAUSE)

    public void OpenMenu() // Mở sidepanel
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME
        SwitchTopLeftButton(isMenuOpen: true);
        ClosePanel(); // Đảm bảo các sub-panel bị tắt trước khi mở Side Menu
        OpenSideMenu(open: true, reset: true);
    }

    public void CloseAllMenus() //Tiếp tục game
    {
        PauseGameManager.SetPause(false); // TIẾP TỤC GAME
        SwitchTopLeftButton(isMenuOpen: false);

        if (_isSidePanelOpen) OpenSideMenu(false);
        ClosePanel(); // Tắt toàn bộ sub-panel
    }

    public enum PanelType
    {
        Settings,
        Credit,
        Play
    }

    public void OpenSubPanel(PanelType type) //Mở các chức năng con
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME (Trường hợp gọi từ script khác)
        SwitchTopLeftButton(isMenuOpen: true);

        if (_isSidePanelOpen) OpenSideMenu(false); // Tắt Side Menu đi
        ClosePanel(); // Đóng các sub-panel khác trước khi mở cái mới

        switch (type)
        {
            case PanelType.Settings: OpenPanel(_settingsPanel, true); break;
            case PanelType.Credit: OpenPanel(_creditPanel, true); break;
            case PanelType.Play: OpenPanel(_playPanel, true); break;
        }
    }
    #endregion

    #region Click Handlers

    private void OnBackClicked()
    {
        if (_settingsPanel.activeSelf || _creditPanel.activeSelf || _playPanel.activeSelf)
        {
            // Đang ở Menu Con -> Tắt Menu Con, Quay về Side Menu
            ClosePanel();
            OpenSideMenu(true);
            // (Không đụng tới PauseGameManager vì vẫn đang ở trong giao diện Menu)
        }
        else if (_isSidePanelOpen)
        {
            // Đang ở Side Menu -> Tắt sạch, quay lại chơi Game
            CloseAllMenus();
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
        btnToHide.transform.DOKill();
        btnToShow.transform.DOKill();

        btnToHide.transform.DOScale(Vector3.zero, _switchDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                btnToHide.gameObject.SetActive(false);
            });

        btnToShow.gameObject.SetActive(true);
        btnToShow.transform.localScale = Vector3.zero;

        btnToShow.transform.DOScale(Vector3.one, _switchDuration + 0.1f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    #endregion

    #region Panel open

    public void OpenSideMenu(bool open, bool reset = false)
    {
        _isSidePanelOpen = open;
        UpdateSelectionUI();

        if (reset) ResetSelection();
        UIHelper.AnimateFade(_sidePanel, open);
    }

    private void OpenPanel(GameObject panel, bool open)
    {
        if (!panel.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        UIHelper.AnimateFade(panel, open);
    }

    private void ClosePanel(GameObject panel = null) //Close one or all panels
    {
        if (panel != null)
        {
            OpenPanel(panel, false);
        }
        else
        {
            if (_settingsPanel.activeSelf) OpenPanel(_settingsPanel, false);
            if (_creditPanel.activeSelf) OpenPanel(_creditPanel, false);
            if (_playPanel.activeSelf) OpenPanel(_playPanel, false);
        }
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