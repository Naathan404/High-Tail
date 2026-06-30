using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;

public class LightGate : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string _targetSceneName;
    [SerializeField] private Vector2 _targetPosition;

    [Header("References")]
    [SerializeField] private Transform _gateCenterPoint;   
    [SerializeField] private CanvasGroup _whiteFlashGroup;   
    [SerializeField] private ParticleSystem _gateIdleParticles;   
    [SerializeField] private ParticleSystem _absorbParticles;     
    [SerializeField] private Light2D _gateLight;            
    //[SerializeField] private SpriteRenderer _playerSprite;    

    [Header("Lore Text")]
    [SerializeField] private TextMeshProUGUI _loreText;
    [SerializeField] private List<LocalizedString> _loreLines;
    [SerializeField] private float _typewriterSpeed = 0.04f; 
    [SerializeField] private float _loreLineHoldTime = 1.2f;   
    [SerializeField] private float _loreLineFadeOut = 0.3f;

    [Header("Timing")]
    [SerializeField] private float _approachZoomDuration = 0.5f;
    [SerializeField] private float _moveToCenterDuration = 3f;
    [SerializeField] private float _absorbDuration = 1.0f;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private float _holdWhiteBeforeLore = 0.3f;
    [SerializeField] private float _holdWhiteAfterLore = 0.5f;

    [Header("Camera")]
    //[SerializeField] private Camera _mainCamera;
    [SerializeField] private float _zoomedOrthoSize = 4f;
    private float _defaultOrthoSize;

    private PlayerController _player;
    private bool _triggered = false;

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
        // if (_mainCamera != null)
        //     _defaultOrthoSize = _mainCamera.orthographicSize;

        if (_whiteFlashGroup != null)
        {
            _whiteFlashGroup.alpha = 0f;
            _whiteFlashGroup.blocksRaycasts = false;
        }

        _loreText.text = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        StartCoroutine(GateSequence());
    }

    private IEnumerator GateSequence()
    {
        InputManager.Instance.DisableControl();


        if (_gateIdleParticles != null)
        {
            var emission = _gateIdleParticles.emission;
            emission.rateOverTimeMultiplier = 0f;
            DOTween.To(() => emission.rateOverTimeMultiplier,
                x => emission.rateOverTimeMultiplier = x,
                30f, _approachZoomDuration);
        }

        if (_gateLight != null)
            DOTween.To(() => _gateLight.intensity, x => _gateLight.intensity = x, 2.5f, _approachZoomDuration);


        yield return new WaitForSeconds(_approachZoomDuration);

        if (_gateCenterPoint != null)
        {
            _player.transform.DOMove(_gateCenterPoint.position, _moveToCenterDuration).SetEase(Ease.InQuad);
        }
        yield return new WaitForSeconds(_moveToCenterDuration);

        _player.enabled = false;

        if (_player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero; 
            rb.gravityScale = 0f;             
        }

        if (_absorbParticles != null)
            _absorbParticles.Play();


        CameraShakeManager.Instance.ShakeCustom(0.4f);
        FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor);

        if (_whiteFlashGroup != null)
            _whiteFlashGroup.DOFade(1f, _absorbDuration).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(_absorbDuration);

        if (_whiteFlashGroup != null)
            _whiteFlashGroup.blocksRaycasts = true;
        yield return new WaitForSeconds(_flashDuration);
        yield return new WaitForSeconds(_holdWhiteBeforeLore);

        if (_loreLines != null && _loreLines.Count > 0 && _loreText != null)
        {
            yield return StartCoroutine(TypewriterLoreSequence());
        }
 
        yield return new WaitForSeconds(_holdWhiteAfterLore);

        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        _player.transform.position = _targetPosition;
        _player.enabled = true;
        rb.gravityScale = _player.BaseGravity;
        SceneTransitionHandler.Instance.LoadSceneAsync(null, _targetSceneName, "M0");


        yield return new WaitForSeconds(_flashDuration);
    }

        private IEnumerator TypewriterLoreSequence()
    {
        foreach (var line in _loreLines)
        {
            string localizedLine = line.GetLocalizedString();
 
            _loreText.text = localizedLine;
            _loreText.maxVisibleCharacters = 0;
            _loreText.alpha = 1f;
 
            float elapsed = 0f;
            float duration = localizedLine.Length * _typewriterSpeed;

            int lastVisibleChar = 0;

            // Gõ từng chữ
            DOTween.To(() => _loreText.maxVisibleCharacters,
                x => _loreText.maxVisibleCharacters = x,
                localizedLine.Length, duration)
                .OnUpdate(() => 
                {
                    if (_loreText.maxVisibleCharacters > lastVisibleChar)
                    {
                        AudioManager.Instance.PlaySFX(SoundName.Typing);
                        lastVisibleChar = _loreText.maxVisibleCharacters;
                    }
                });
 
            yield return new WaitForSeconds(duration);
            yield return new WaitForSeconds(_loreLineHoldTime);
 
            yield return _loreText.DOFade(0f, _loreLineFadeOut).WaitForCompletion();
        }
 
        _loreText.text = "";
    }
}