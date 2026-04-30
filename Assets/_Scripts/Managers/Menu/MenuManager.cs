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
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button backButton;
    [SerializeField] private float _switchDuration = 0.25f;

    [Header("Side Panel")]
    [SerializeField] private GameObject sidePanel;
    private CanvasGroup sidePanelCanvasGroup;
    private float fadeDuration = 0.5f;

    [Header("Navigation")]
    [SerializeField] private Button newButton;
    [SerializeField] private Button contButton;
    [SerializeField] private Button setButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button exitButton;
    private List<Button> sidePanelButtons = new List<Button>();
    [SerializeField] private int currentButtonIndex = 0;
    private bool isSidePanelOpen = false;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private GameObject continuePanel;

    public override void Awake()
    {
        base.Awake();
        if (sidePanel != null)
        {
            if (!sidePanel.TryGetComponent<CanvasGroup>(out sidePanelCanvasGroup))
            {
                sidePanelCanvasGroup = sidePanel.AddComponent<CanvasGroup>();
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
        sidePanelButtons = new List<Button> {
            contButton,
            newButton,
            setButton,
            creditButton,
            exitButton 
        };

        // Khởi tạo ban đầu: Đảm bảo mọi Menu đều đóng và Game đang chạy
        // (Nếu đây là Scene Main Menu riêng biệt, bạn có thể đổi thành OpenMenu() nhé)
        CloseAllMenus(); 
    }

    private void SetupButtonListeners()
    {
        // Gán trực tiếp vào các API công khai cho gọn code
        pauseButton.onClick.AddListener(OpenMenu);
        backButton.onClick.AddListener(OnBackClicked);
        newButton.onClick.AddListener(OnNewGameClicked);
        
        contButton.onClick.AddListener(() => OpenSubPanel(PanelType.Continue));
        setButton.onClick.AddListener(() => OpenSubPanel(PanelType.Settings));
        creditButton.onClick.AddListener(() => OpenSubPanel(PanelType.Credit));
        
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        // 1. Logic bật/tắt bằng phím (ESC)
        if (InputManager.Instance.Inputs.UI.Menu.WasPressedThisFrame())
        {
            if (isSidePanelOpen || settingsPanel.activeSelf || creditPanel.activeSelf || continuePanel.activeSelf)
            {
                OnBackClicked();
            }
            else
            {
                OpenMenu(); 
            }
        }

        if (!isSidePanelOpen || sidePanelButtons.Count == 0) return;

        // 2. Logic Reset lựa chọn khi rê chuột
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            RectTransform panelRect = sidePanel.GetComponent<RectTransform>();
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePosition, null))
            {
                if (currentButtonIndex != -1)
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
            
            if (currentButtonIndex != -1 &&
                (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                sidePanelButtons[currentButtonIndex].onClick.Invoke();
            }
        }
    }

    #region Public API (LUỒNG CHÍNH ĐỂ QUẢN LÝ PAUSE/UNPAUSE)

    public void OpenMenu()
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME
        SwitchTopLeftButton(isMenuOpen: true);
        ClosePanel(); // Đảm bảo các sub-panel bị tắt trước khi mở Side Menu
        OpenSideMenu(open: true, reset: true);
    }

    public void CloseAllMenus()
    {
        PauseGameManager.SetPause(false); // TIẾP TỤC GAME
        SwitchTopLeftButton(isMenuOpen: false);
        
        if (isSidePanelOpen) OpenSideMenu(false);
        ClosePanel(); // Tắt toàn bộ sub-panel
    }

    public enum PanelType
    {
        Settings,
        Credit,
        Continue
    }

    public void OpenSubPanel(PanelType type)
    {
        PauseGameManager.SetPause(true); // NGỪNG GAME (Trường hợp gọi từ script khác)
        SwitchTopLeftButton(isMenuOpen: true);
        
        if (isSidePanelOpen) OpenSideMenu(false); // Tắt Side Menu đi
        ClosePanel(); // Đóng các sub-panel khác trước khi mở cái mới

        switch (type)
        {
            case PanelType.Settings: OpenPanel(settingsPanel, true); break;
            case PanelType.Credit: OpenPanel(creditPanel, true); break;
            case PanelType.Continue: OpenPanel(continuePanel, true); break;
        }
    }
    #endregion

    #region Click Handlers

    private void OnBackClicked()
    {
        if (settingsPanel.activeSelf || creditPanel.activeSelf || continuePanel.activeSelf)
        {
            // Đang ở Menu Con -> Tắt Menu Con, Quay về Side Menu
            ClosePanel();          
            OpenSideMenu(true);    
            // (Không đụng tới PauseGameManager vì vẫn đang ở trong giao diện Menu)
        }
        else if (isSidePanelOpen)
        {
            // Đang ở Side Menu -> Tắt sạch, quay lại chơi Game
            CloseAllMenus(); 
        }
    }

    private void OnNewGameClicked()
    {
        CloseAllMenus(); 
        SaveManager.Instance.StartNewGame();
    }

    private void OnExitClicked()
    {
        //TODO: Show confirmation dialog before quitting
        Application.Quit();
    }
    #endregion

    #region On Game Button
    private void SwitchTopLeftButton(bool isMenuOpen)
    {
        if (isMenuOpen)
        {
            AnimateButtonSwitch(pauseButton, backButton);
        }
        else
        {
            AnimateButtonSwitch(backButton, pauseButton);
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
        isSidePanelOpen = open;
        sidePanelCanvasGroup.DOKill();

        if (open)
        {
            UpdateSelectionUI();
            if (reset) ResetSelection();

            sidePanel.SetActive(true);
            sidePanelCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        }
        else
        {
            sidePanelCanvasGroup.DOFade(0f, fadeDuration)
                                .SetUpdate(true)
                                .OnComplete(() =>
                                {
                                    sidePanel.SetActive(false);
                                });
        }
    }

    private void OpenPanel(GameObject panel, bool open)
    {
        if (!panel.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
        {
            cg = panel.AddComponent<CanvasGroup>();
        }

        cg.DOKill();

        if (open)
        {
            panel.SetActive(true);
            cg.alpha = 0f; 
            cg.DOFade(1f, fadeDuration).SetUpdate(true);
        }
        else
        {
            cg.DOFade(0f, fadeDuration)
              .SetUpdate(true)
              .OnComplete(() => panel.SetActive(false));
        }
    }

    private void ClosePanel(GameObject panel = null)
    {
        if (panel != null)
        {
            OpenPanel(panel, false); 
        }
        else
        {
            if (settingsPanel.activeSelf) OpenPanel(settingsPanel, false);
            if (creditPanel.activeSelf) OpenPanel(creditPanel, false);
            if (continuePanel.activeSelf) OpenPanel(continuePanel, false);
        }
    }

    #endregion

    #region Handle Keyboard Selection
    public void ResetSelection()
    {
        currentButtonIndex = -1;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void SelectPrevious()
    {
        if (sidePanelButtons == null || sidePanelButtons.Count == 0) return;

        if (currentButtonIndex == -1)
        {
            currentButtonIndex = sidePanelButtons.Count - 1;
        }
        else
        {
            currentButtonIndex--;
            if (currentButtonIndex < 0)
            {
                currentButtonIndex = sidePanelButtons.Count - 1;
            }
        }
        UpdateSelectionUI();
    }

    private void SelectNext()
    {
        if (sidePanelButtons == null || sidePanelButtons.Count == 0) return;

        if (currentButtonIndex == -1)
        {
            currentButtonIndex = 0;
        }
        else
        {
            currentButtonIndex++;
            if (currentButtonIndex >= sidePanelButtons.Count)
            {
                currentButtonIndex = 0;
            }
        }
        UpdateSelectionUI();
    }

    private void UpdateSelectionUI()
    {
        if (currentButtonIndex >= 0 && currentButtonIndex < sidePanelButtons.Count)
        {
            EventSystem.current.
                SetSelectedGameObject(sidePanelButtons[currentButtonIndex].gameObject);
        }
    }
    #endregion
}