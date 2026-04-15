using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : Singleton<SceneTransitionManager>
{
    [Header("UI ")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private float _fadeDuration = 0.5f;
    
    private bool _isTransitioning = false;

    private void Start()
    {
        _fadeCanvasGroup.alpha = 0f;
        _fadeCanvasGroup.blocksRaycasts = false;
    }

    public void DoTransition(string sceneToUnload, string SceneToLoad, string doorID)
    {
        if(_isTransitioning) return;
        StartCoroutine(LoadSceneTransitionAsync(sceneToUnload, SceneToLoad, doorID));
    }

    private IEnumerator LoadSceneTransitionAsync(string sceneToUnload, string SceneToLoad, string doorID)
    {
        // set up for transition
        _isTransitioning = true;
        _fadeCanvasGroup.blocksRaycasts = true;

        // fade out using dotween
        yield return _fadeCanvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();

        // load the new scene
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneToLoad));

        // teleport the player to the target door position in new scene
        TeleportPlayer(doorID);

        // unload the current scene
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
        while(!unloadOp.isDone) yield return null;

        // fade in using dotween
        yield return _fadeCanvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();

        // after transition
        _isTransitioning = false;
        _fadeCanvasGroup.blocksRaycasts = false;
        InputManager.Instance.EnableControl();
    }

    private void TeleportPlayer(string doorID)
    {
        DoorTransition[] doors = Object.FindObjectsByType<DoorTransition>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var door in doors)
        {
            if(door.DoorID == doorID)
            {
                PlayerController player = FindAnyObjectByType<PlayerController>();
                if(player != null)
                {
                    player.transform.position = door.SpawnPoint.position;
                    Debug.Log($"Dịch chuyển người chơi đến cửa có ID: {doorID}");
                    return;
                }
            }
        }
        Debug.Log($"Không tìm thấy cửa nào có ID: {doorID}");
    }
}
