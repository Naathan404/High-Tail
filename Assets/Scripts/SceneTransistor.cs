using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransistor : MonoBehaviour
{
    public static SceneTransistor Instance; // Singleton

    [Header("Scene Transition Settings")]
    [Tooltip("The scene that we load into")]
    [SerializeField] private string _nextScene;
    [Tooltip("The transition effect when we load a new scene")]
    [SerializeField] private TransitionType _loadSceneTransitionType;
    [Tooltip("The transition effect when this scene is loaded")]
    [SerializeField] private TransitionType _onLoadedTransitionType;
    [SerializeField] private float _transitionDuration;
    [Tooltip("The time to load scene before the end of transition duration")]
    [SerializeField] private float _modifyValue = 0.1f;

    private float _animationSpeed;
    private Animator _animator;
    public enum TransitionType
    {
        FadeIn,
        FadeOut,
        WideIn,
        WideOut,
        SwipeIn,
        SwipeOut
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
        _animationSpeed = 1f / _transitionDuration; // Calculate the animation speed based on the desired transition duration
        _animator.speed = _animationSpeed; // Set the animator speed to achieve the desired transition duration
    }

    private void Start()
    {
        OnSceneLoaded();
    }

    // Load a new scene with the specified transition effect
    public void LoadScene()
    {
        StartCoroutine(TransitionToNextScene());   
    }

    // Turn on the transition effect when this scene is loaded
    public void OnSceneLoaded()
    {
        ApplyTransitionEffect(_onLoadedTransitionType);
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
        ApplyTransitionEffect(_loadSceneTransitionType);

        yield return new WaitForSeconds(modifiedDuration);
        SceneManager.LoadScene(_nextScene);
    }

    private void ApplyTransitionEffect(TransitionType transitionType)
    {
        switch (transitionType)
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
            case TransitionType.SwipeIn:
                {
                    SwipeIn();
                    break;
                }
            case TransitionType.SwipeOut:
                {
                    SwipeOut();
                    break;
                }
        }
    }

    #region Transition effects
    private void FadeIn()
    {
        _animator.SetTrigger("FadeIn");
    }

    private void FadeOut()
    {
        _animator.SetTrigger("FadeOut");
    }

    private void WideIn()
    {
        _animator.SetTrigger("WideIn");
    }

    private void WideOut()
    {
        _animator.SetTrigger("WideOut");
    }
    private void SwipeIn()
    {
        _animator.SetTrigger("SwipeIn");
    }

    private void SwipeOut()
    {
        _animator.SetTrigger("SwipeOut");
    }
    #endregion
}
