using System;
using System.Collections;
using UnityEngine;
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
    [SerializeField] private Button startButton;
    [SerializeField] private Button newButton;
    [SerializeField] private Button contButton;
    [SerializeField] private Button setButton;
    [SerializeField] private Button inforButton;
    [SerializeField] private Button exitButton;

    private void Start()
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

        startButton.gameObject.SetActive(true);
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
    public void OpenSideMenu(bool open)
    {
        if (open)
        {
            ResetButtonScales();
            sidePanel.SetActive(true);
            StartCoroutine(DoFade(sidePanelCanvasGroup, 0f, 1f));
        }
        else
        {
            StartCoroutine(DoFade(sidePanelCanvasGroup, 1f, 0f, () =>
            {
                sidePanel.SetActive(false);
                pauseButton.gameObject.SetActive(true);
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
        if (sidePanel == null) return;
        Button[] allButtons = sidePanel.GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            btn.transform.localScale = Vector3.one;
        }
    }
}
