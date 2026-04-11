using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    [SerializeField] private Button inforButton;
    [SerializeField] private Button exitButton;
    [Header("Navigation")]
    private List<Button> sidePanelButtons = new List<Button>();
    [SerializeField] private int currentButtonIndex = 0;
    private bool isSidePanelOpen = false;

    void Start()
    {
        if (sidePanel != null)
        {
            sidePanelCanvasGroup = sidePanel.GetComponent<CanvasGroup>();
            if (sidePanelCanvasGroup == null)
            {
                sidePanelCanvasGroup = sidePanel.AddComponent<CanvasGroup>();
            }
            sidePanelCanvasGroup.alpha = 0f;
            sidePanel.SetActive(false);
        }

        pauseButton.gameObject.SetActive(true);
        pauseButton.onClick.AddListener(() =>
        {
            OpenSideMenu(true);
            pauseButton.gameObject.SetActive(false);
        });

        backButton.gameObject.SetActive(true);
        backButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });

        sidePanelButtons.AddRange(new Button[] { newButton, contButton, setButton, inforButton, exitButton });

        newButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });
        contButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });
        setButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });
        inforButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });
        exitButton.onClick.AddListener(() =>
        {
            OpenSideMenu(false);
        });
    }

    void Update()
    {
        if (!isSidePanelOpen || sidePanelButtons.Count == 0) return;
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
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                sidePanelButtons[currentButtonIndex].onClick.Invoke();
            }
        }

    }
    public void OpenSideMenu(bool open)
    {
        isSidePanelOpen = open;
        if (open)
        {
            currentButtonIndex = 0;
            UpdateSelectionUI();
            ResetButtonScales();
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

    private void ResetButtonScales()
    {
        if (sidePanelButtons == null) return;
        foreach (Button btn in sidePanelButtons)
        {
            btn.transform.localScale = Vector3.one;
        }
    }

    private void SelectPrevious()
    {
        currentButtonIndex--;
        if (currentButtonIndex < 0)
        {
            currentButtonIndex = sidePanelButtons.Count - 1;
        }
        UpdateSelectionUI();
    }

    private void SelectNext()
    {
        currentButtonIndex++;
        if (currentButtonIndex >= sidePanelButtons.Count)
        {
            currentButtonIndex = 0;
        }
        UpdateSelectionUI();
    }

    private void UpdateSelectionUI()
    {
        EventSystem.current.SetSelectedGameObject(sidePanelButtons[currentButtonIndex].gameObject);
    }
}
