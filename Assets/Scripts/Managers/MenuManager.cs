using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button backButton;
    [Header("Side Panel")]
    [SerializeField] private GameObject sidePanel;
    private CanvasGroup sidePanelCanvasGroup;
    private float fadeDuration = 0.5f;
    [Header("Button Actions")]
    [SerializeField] private Button newButton;
    [SerializeField] private Button contButton;
    [SerializeField] private Button setButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button exitButton;
    [Header("Navigation")]
    private List<Button> sidePanelButtons = new List<Button>();
    [SerializeField] private int currentButtonIndex = 0;
    private bool isSidePanelOpen = false;
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private GameObject continuePanel;
    [SerializeField] private GameObject onGamePanel;

    private void Awake()
    {
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
        settingsPanel.SetActive(false);
        creditPanel.SetActive(false);
        continuePanel.SetActive(false);

        pauseButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        sidePanelButtons = new List<Button> { contButton, newButton, setButton, creditButton, exitButton };

        OpenSideMenu(true, true);
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

    #region Click Handlers
    private void OnPauseClicked()
    {
        OpenSideMenu(true);
        ClosePanel();
        pauseButton.gameObject.SetActive(false);
    }

    private void OnBackClicked()
    {
        OpenSideMenu(false);
    }

    private void OnNewGameClicked()
    {
        OpenSideMenu(false);
        SceneManager.LoadScene("[OFF] SCENE_1");
    }

    private void OnContinueClicked()
    {
        OpenSideMenu(false);
        //TODO: Load last checkpoint or save data
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
        OpenSideMenu(false);
        //TODO: Show confirmation dialog before quitting
        Application.Quit();
    }
    #endregion

    void Update()
    {
        if (!isSidePanelOpen || sidePanelButtons.Count == 0) return;
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            if (currentButtonIndex != -1)
            {
                currentButtonIndex = -1;
                EventSystem.current.SetSelectedGameObject(null);
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
    public void OpenSideMenu(bool open, bool reset = false)
    {
        isSidePanelOpen = open;
        if (open)
        {
            ResetButtonScales();
            UpdateSelectionUI();
            if (reset) ResetSelection();
            sidePanel.SetActive(true);
            //PauseGameManager.SetPause(true);
            StartCoroutine(DoFade(sidePanelCanvasGroup, 0f, 1f));
        }
        else
        {
            pauseButton.gameObject.SetActive(true);
            StartCoroutine(DoFade(sidePanelCanvasGroup, 1f, 0f, () =>
            {
                sidePanel.SetActive(false);
                //PauseGameManager.SetPause(false);
            }));
        }
    }

    private void OpenPanel(GameObject panel, bool open)
    {
        if (open)
        {
            panel.SetActive(true);
            StartCoroutine(DoFade(panel.GetComponent<CanvasGroup>(), 0f, 1f));
        }
        else
        {
            StartCoroutine(DoFade(panel.GetComponent<CanvasGroup>(), 1f, 0f, () =>
            {
                panel.SetActive(false);
            }));
        }
    }

    private void ClosePanel(GameObject panel = null)
    {
        if (panel != null)
        {
            StartCoroutine(DoFade(panel.GetComponent<CanvasGroup>(), 1f, 0f, () =>
            {
                panel.SetActive(false);
            }));
        }
        else
        {
            if (settingsPanel.activeSelf)
                OpenPanel(settingsPanel, false);
            if (creditPanel.activeSelf)
                OpenPanel(creditPanel, false);
            if (continuePanel.activeSelf)
                OpenPanel(continuePanel, false);
        }
    }

    private IEnumerator DoFade(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, Action onComplete = null)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    #region Side Panel Button Effects
    private void ResetButtonScales()
    {
        if (sidePanelButtons == null) return;
        foreach (Button btn in sidePanelButtons)
        {
            btn.transform.localScale = Vector3.one;
        }
    }
    public void ResetSelection()
    {
        currentButtonIndex = -1;
        EventSystem.current.SetSelectedGameObject(null);
        ResetButtonScales();
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
            EventSystem.current.SetSelectedGameObject(sidePanelButtons[currentButtonIndex].gameObject);
        }
    }
    #endregion
}
