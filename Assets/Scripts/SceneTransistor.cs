using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransistor : MonoBehaviour
{
    public static SceneTransistor Instance; // Singleton
    [SerializeField] private string _nextScene;
    [SerializeField] private TransitionType _loadSceneTransitionType;
    [SerializeField] private TransitionType _onLoadedTransitionType;
    [SerializeField] private float _transitionDuration;
    [SerializeField] private float _modifyValue = 0.1f;
    private Animator _animator;
    public enum TransitionType
    {
        FadeIn,
        FadeOut,
        WideIn,
        WideOut
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }   
        _animator = GetComponent<Animator>();
        OnSceneLoaded();
    }

    // Load a new scene with the specified transition effect
    public void LoadScene()
    {
        Debug.Log("LoadScene");
        StartCoroutine(TransitionToNextScene());   
    }

    // Turn on the transition effect when this scene is loaded
    public void OnSceneLoaded()
    {
        switch (_onLoadedTransitionType)
        {
            case TransitionType.FadeIn:
                {
                    FadeIn();
                    break;
                }
            case TransitionType.FadeOut:
                {
                    FadeOut();
                    break;
                }
            case TransitionType.WideIn:
                {
                    WideIn();
                    break;
                }
            case TransitionType.WideOut:
                {
                    WideOut();
                    break;
                }
        }
    }

    IEnumerator TransitionToNextScene() 
    {
        // Logic
        if (string.IsNullOrEmpty(_nextScene))
        {
            Debug.LogError("Scene name is null or empty.");
            yield return null;
        }

        float modifiedDuration = _transitionDuration - _modifyValue;
        switch (_loadSceneTransitionType)
        {
            case TransitionType.FadeIn:
                {
                    FadeIn();
                    yield return new WaitForSeconds(modifiedDuration);
                    break;
                }
            case TransitionType.FadeOut:
                {
                    FadeOut();
                    yield return new WaitForSeconds(modifiedDuration);
                    break;
                }
            case TransitionType.WideIn:
                {
                    WideIn();
                    yield return new WaitForSeconds(modifiedDuration);
                    break;
                }
            case TransitionType.WideOut:
                {
                    WideOut();
                    yield return new WaitForSeconds(modifiedDuration);
                    break;
                }
        }
        SceneManager.LoadScene(_nextScene);
    }

    #region Transition effects
    public void FadeIn()
    {
        _animator.SetTrigger("FadeIn");
    }

    public void FadeOut()
    {
        _animator.SetTrigger("FadeOut");
    }

    public void WideIn()
    {
        _animator.SetTrigger("WideIn");
    }

    public void WideOut()
    {
        _animator.SetTrigger("WideOut");
    }
    #endregion
}
