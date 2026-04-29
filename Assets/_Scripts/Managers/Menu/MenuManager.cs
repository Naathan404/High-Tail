using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
            exitButton };

        ClosePanel();
        OpenSideMenu(open: true, reset: true);
    }

    private void SetupButtonListeners()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        backButton.onClick.AddListener(OnBackClicked);
        newButton.onClick.AddListener(OnNewGameClicked);
        contButton.onClick.AddListener(OnContinueClicked);
        setButton.onClick.AddListener(OnSettingsClicked);
        creditButton.onClick.AddListener(OnCreditClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        //Keyboard
        if (InputManager.Instance.Inputs.UI.Menu.WasPressedThisFrame())
        {
            // Tận dụng hàm OnBackClicked vì nó đã có sẵn logic đóng sub-panel hoặc tắt main menu
            if (isSidePanelOpen || settingsPanel.activeSelf || creditPanel.activeSelf || continuePanel.activeSelf)
            {
                OnBackClicked();
            }
            else
            {
                // Mở menu nếu đang ở trong game
                OnPauseClicked(); 
            }
        }
        if (!isSidePanelOpen || sidePanelButtons.Count == 0) return;
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            RectTransform panelRect = sidePanel.GetComponent<RectTransform>();
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (panelRect != null && RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePosition, null))
            {
                if (currentButtonIndex != -1)
                {
                    currentButtonIndex = -1;
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
        if (Keyboard.current == null) return;
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
                Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                sidePanelButtons[currentButtonIndex].onClick.Invoke();
            }
        }

    }

    #region Public API
    public void OpenMenu()
    {
        SwitchTopLeftButton(isMenuOpen: true);
        OpenSideMenu(open: true, reset: true);
    }

    public enum PanelType
    {
        Settings,
        Credit,
        Continue
    }

    public void OpenSubPanel(PanelType type)
    {
        SwitchTopLeftButton(isMenuOpen: true);
        switch (type)
        {
            case PanelType.Settings:
                OnSettingsClicked();
                break;
            case PanelType.Credit:
                OnCreditClicked();
                break;
            case PanelType.Continue:
                OnContinueClicked();
                break;
        }
    }

    public void CloseMenu()
    {
        if (isSidePanelOpen)
        {
            OpenSideMenu(false);
        }
        SwitchTopLeftButton(isMenuOpen: false);
        ClosePanel();
    }

    #endregion

    #region Click Handlers
    private void OnPauseClicked()
    {
        SwitchTopLeftButton(isMenuOpen: true);
        ClosePanel();
        OpenSideMenu(open: true);
    }

    private void OnBackClicked()
    {
        if (settingsPanel.activeSelf || creditPanel.activeSelf || continuePanel.activeSelf)
        {
            ClosePanel();          // Đóng Menu con
            OpenSideMenu(true);    // Bật lại Side Menu
            //SwitchTopLeftButton(true); // Vẫn giữ nút Back
        }
        else if (isSidePanelOpen)
        {
            OpenSideMenu(false);   // Tắt Side Menu
            SwitchTopLeftButton(false); // Đổi về nút Pause (Về lại Game)
        }
    }

    private void OnNewGameClicked()
    {
        OpenSideMenu(false);
        SwitchTopLeftButton(false);
        SaveManager.Instance.StartNewGame();
    }

    private void OnContinueClicked()
    {
        OpenSideMenu(false);
        OpenPanel(continuePanel, true);
        Debug.Log("Continue game");
    }

    private void OnSettingsClicked()
    {
        OpenSideMenu(false);
        OpenPanel(settingsPanel, true);
    }

    private void OnCreditClicked()
    {
        OpenSideMenu(false);
        OpenPanel(creditPanel, true);
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
                btnToHide.gameObject.SetActive(false); // Chỉ tắt hẳn khi đã thu nhỏ xong
            });


        btnToShow.gameObject.SetActive(true); // Bật lên trước
        btnToShow.transform.localScale = Vector3.zero; // Ép về 0 trước khi phóng

        btnToShow.transform.DOScale(Vector3.one, _switchDuration + 0.1f) // Bật lên chậm hơn lúc tắt một chút cho đẹp
            .SetEase(Ease.OutBack) // Nảy lên quá đà một xíu rồi bật về vị trí cũ
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
            // TODO: PauseGameManager.SetPause(true);

            sidePanelCanvasGroup.DOFade(1f, fadeDuration)
                                .SetUpdate(true);
        }
        else
        {
            sidePanelCanvasGroup.DOFade(0f, fadeDuration)
                                .SetUpdate(true)
                                .OnComplete(() =>
                                {
                                    sidePanel.SetActive(false);
                                    //TODO: PauseGameManager.SetPause(false);
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
            cg.alpha = 0f; // Đảm bảo bắt đầu từ 0
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
            OpenPanel(panel, false); // Tận dụng luôn hàm OpenPanel ở trên cho gọn
        }
        else
        {
            // Tắt tất cả các bảng đang mở
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
