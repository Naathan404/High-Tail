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

    [Header("Absorb Effect")]
    [SerializeField] private ParticleSystem _absorbParticles;     
    [SerializeField] private float _absorbTravelDuration = 1f;

    [Header("Player Dissolve")]
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private float _dissolveDuration = 0.5f;

    

    [Header("Gate Charge (Nhịp 1 - anticipation)")]
    [SerializeField] private Light2D _gateLight;
    [SerializeField] private float _chargeUpDuration = 2.5f;      // thời gian cổng "kéo" trước khi nhân vật tan biến
    [SerializeField] private float _gateLightMaxIntensity = 3f;
    [SerializeField] private float _playerPullStrength = 1.5f;    // lực nhỏ hút nhân vật về phía cổng (tạo cảm giác bị kéo)
    [SerializeField] private float _playerShakeStrength = 0.08f;  // rung nhẹ nhân vật lúc bị kéo


    //[SerializeField] private SpriteRenderer _playerSprite;    

    [Header("Lore Text")]
    [SerializeField] private TextMeshProUGUI _loreText;
    [SerializeField] private List<LocalizedString> _loreLines;
    [SerializeField] private float _typewriterSpeed = 0.04f; 
    [SerializeField] private float _loreLineHoldTime = 1.2f;   
    [SerializeField] private float _loreLineFadeOut = 0.3f;

    [Header("Timing")]
    // [SerializeField] private float _approachZoomDuration = 0.5f;
    // [SerializeField] private float _moveToCenterDuration = 3f;
    // [SerializeField] private float _absorbDuration = 1.0f;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private float _holdWhiteBeforeLore = 0.3f;
    [SerializeField] private float _holdWhiteAfterLore = 0.5f;

    private PlayerController _player;
    private Rigidbody2D _rb;


    [SerializeField] private GameObject _interactMark;
    private bool _canInteract = false;

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();

        if (_player != null)
        {
            _rb = _player.GetComponent<Rigidbody2D>();
            _playerSprite = FindAnyObjectByType<PlayerVisual>().GetComponent<SpriteRenderer>();
        }

        if (_whiteFlashGroup != null)
        {
            _whiteFlashGroup.alpha = 0f;
            _whiteFlashGroup.blocksRaycasts = false;
        }

        _loreText.text = "";


        _interactMark.transform.DOMoveY(_interactMark.transform.position.y + 0.2f, 0.25f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _canInteract = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _canInteract = false;
        }
    }

    private void Update()
    {
        _interactMark.SetActive(_canInteract);
        if (_canInteract && InputManager.Instance.Inputs.Interaction.Interact.WasPressedThisFrame())
        {
            _canInteract = false;
            StartCoroutine(GateSequence());
        }
    }

    private IEnumerator GateSequence()
    {
        Debug.Log($"[LightGate] Time.timeScale = {Time.timeScale}");
        InputManager.Instance.DisableControl();
        _player.enabled = false;

        if (_player.IsOnGround())
            _player.StateMachine.ChangeState(_player.IdleState);
        else
            _player.StateMachine.ChangeState(_player.FallState);

        if (_gateLight != null)
            DOTween.To(() => _gateLight.intensity, x => _gateLight.intensity = x,
                _gateLightMaxIntensity, _chargeUpDuration).SetEase(Ease.InQuad);

        if (_gateIdleParticles != null)
        {
            var emission = _gateIdleParticles.emission;
            DOTween.To(() => emission.rateOverTimeMultiplier,
                x => emission.rateOverTimeMultiplier = x,
                30f, _chargeUpDuration);
        }

        CameraShakeManager.Instance.ShakeCustom(0.5f);
        FilterManager.Instance.FlashVignette(FilterManager.Instance.FlashColor, 1f, _chargeUpDuration);

        StartCoroutine(PullPlayerRoutine(_chargeUpDuration));

        yield return new WaitForSeconds(_chargeUpDuration);

        // ── NHỊP 2: Nhân vật tan biến thành particle ──────────────────────────
        // Đặt _absorbParticles tại vị trí nhân vật rồi burst
        CameraShakeManager.Instance.ShakeCustom(0.3f);
        if (_absorbParticles != null)
        {
            _absorbParticles.transform.position = _player.transform.position;
            var main = _absorbParticles.main;
            main.useUnscaledTime = true;
            _absorbParticles.Play();
        }

        // Fade nhân vật ra
        if (_playerSprite != null)
            _playerSprite.DOFade(0f, _dissolveDuration);

        yield return new WaitForSeconds(_dissolveDuration);

        // Hút đống particle về tâm cổng
        if (_absorbParticles != null && _gateCenterPoint != null)
            _absorbParticles.transform.DOMove(_gateCenterPoint.position, _absorbTravelDuration)
                .SetEase(Ease.InQuad);

        // Camera shake khi particle bay về

        yield return new WaitForSeconds(_absorbTravelDuration);

        // ── NHỊP 3: Flash + Lore + Load scene ────────────────────────────────
        FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor);

        if (_whiteFlashGroup != null)
            _whiteFlashGroup.DOFade(1f, _flashDuration).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(_flashDuration);

        if (_whiteFlashGroup != null)
            _whiteFlashGroup.blocksRaycasts = true;

        yield return new WaitForSeconds(_holdWhiteBeforeLore);

        if (_loreLines != null && _loreLines.Count > 0 && _loreText != null)
            yield return StartCoroutine(TypewriterLoreSequence());

        yield return new WaitForSeconds(_holdWhiteAfterLore);

        // Reset trước khi load
        _player.transform.position = _targetPosition;
        _player.enabled = true;
        // if (_rb != null)
        //     _rb.gravityScale = _player.BaseGravity;

        // Restore sprite alpha cho scene mới (SceneLoader sẽ fade in từ trắng)
        if (_playerSprite != null)
        {
            Color c = _playerSprite.color;
            _playerSprite.color = new Color(c.r, c.g, c.b, 1f);
        }

        AudioManager.Instance.StopSFX();

        SceneTransitionHandler.Instance.LoadSceneAsync(null, _targetSceneName, "M0");
    }

    private IEnumerator PullPlayerRoutine(float duration)
    {
        float elapsed = 0f;
        _player.StateMachine.ChangeState(_player.FallState);
        AudioManager.Instance.PlaySFX(SoundName.Reward);
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero; 
            _rb.gravityScale = 0f;             
        }
        _player.transform.DOPunchScale(
            Vector3.one * _playerShakeStrength, duration, vibrato: 15, elasticity: 0.5f);

        while (elapsed < duration)
        {
            if (_rb != null && _gateCenterPoint != null)
            {
                Vector2 dir = ((Vector2)_gateCenterPoint.position - _rb.position).normalized;

                float t = elapsed / duration;
                _rb.MovePosition(_rb.position + dir * _playerPullStrength * t * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
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