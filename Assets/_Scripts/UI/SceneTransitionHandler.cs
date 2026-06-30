using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : Singleton<SceneTransitionHandler>
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _waitTime = 1f; 

    [SerializeField] private RectTransform _loadingIcon;

    private bool _isTransitioning = false;

    private void Start()
    {
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0f;

        _loadingIcon.DORotate(new Vector3(0f, 0f, -360f), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
    }

    public void LoadSceneAsync(string sceneToLoad, string sceneToLoadAdditive = null, string sceneToUnload = null, Action onMidpoint = null, Action onComplete = null)
    {
        if (_isTransitioning) return; 
        _isTransitioning = true;
        StartCoroutine(LoadSceneAsyncRoutine(sceneToLoad, sceneToLoadAdditive, sceneToUnload, onMidpoint, onComplete));
    }

    IEnumerator LoadSceneAsyncRoutine(string sceneToLoad, string sceneToLoadAdditive = null, string sceneToUnload = null, Action onMidpoint = null, Action onComplete = null)
    {
        _canvasGroup.blocksRaycasts = true;
        yield return _canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.InOutQuad).SetUpdate(true).WaitForCompletion();

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;
            
            Scene targetScene = SceneManager.GetSceneByName(sceneToLoad);
            if (targetScene.isLoaded) SceneManager.SetActiveScene(targetScene);
        }

        if (!string.IsNullOrEmpty(sceneToLoadAdditive))
        {
            AsyncOperation loadAdditiveOp = SceneManager.LoadSceneAsync(sceneToLoadAdditive, LoadSceneMode.Additive);
            while (!loadAdditiveOp.isDone) yield return null;
        }

        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone) yield return null;
            }
            else
            {
                Debug.LogWarning($"Không thể Unload Scene '{sceneToUnload}'. Có thể nó không tồn tại hoặc sai tên!");
            }
        }

        Debug.Log(">>> BẮT ĐẦU GỌI ON MIDPOINT <<<");
        onMidpoint?.Invoke();

        yield return new WaitForSecondsRealtime(_waitTime);
        yield return _canvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InOutQuad).SetUpdate(true).WaitForCompletion();
        InputManager.Instance.EnableControl();
        _canvasGroup.blocksRaycasts = false;
        _isTransitioning = false;

        onComplete?.Invoke();
    }
}