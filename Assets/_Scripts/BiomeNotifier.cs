using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class BiomeNotifier : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private string _biomeName;
    //[SerializeField] private string biomeDescription;
    [SerializeField] private float _displayDuration = 3f;
    [SerializeField] private float _fadeInDuration = 2f;
    [SerializeField] private float _fadeOutDuration = 2f;
    [SerializeField] private TextMeshProUGUI _biomeText;

    [Header("Triggers")]
    [SerializeField] private Trigger _mainTrigger;
    [SerializeField] private Trigger _triggerReset;
    private bool _hasBeenTriggered = false;

    private void Awake()
    {
        if (_mainTrigger == null)
        {
            Debug.LogWarning("There is no main trigger for biome notifier");
        }
        if (_triggerReset == null)
        {
            Debug.LogWarning("There is no sub trigger for biome notifier");
        }
        _hasBeenTriggered = false;

        if (_biomeText == null)
        {
            Debug.LogWarning("Biome Text is not assigned in the inspector.");
        }

        // Invisible the text at the start
        _biomeText.DOFade(0f, 0f);
    }

    private void OnEnable()
    {
        if (_mainTrigger != null)
        {
            _mainTrigger.OnStepIn += ShowBiomeText;
        }
        if (_triggerReset != null)
        {
            _triggerReset.OnStepIn += ResetTrigger;
        }
    }

    private void OnDisable()
    {
        if (_mainTrigger != null)
        {
            _mainTrigger.OnStepIn -= ShowBiomeText;
        }
        if (_triggerReset != null)
        {
            _triggerReset.OnStepIn -= ResetTrigger;
        }
    }

    private void ShowBiomeText()
    {
        if (_hasBeenTriggered)
        {
            return;
        }
        _hasBeenTriggered = true;

        _biomeText.text = _biomeName;
        _biomeText.text.ToUpper();
        StartCoroutine(TextCoroutine());
    }

    private IEnumerator TextCoroutine()
    {
        AudioManager.Instance.PlaySFX(SoundName.BiomeNotifier);

        _biomeText.DOFade(1f, _fadeInDuration)
                   .SetEase(Ease.InSine);
        yield return new WaitForSeconds(_displayDuration);
        _biomeText.DOFade(0f, _fadeOutDuration);

        AudioManager.Instance.StopSFX();
    }

    private void ResetTrigger()
    {
        _hasBeenTriggered = false;
    }
}
