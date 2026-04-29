using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PhantomPlatform : MonoBehaviour, ILightPulseReactive
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _remainSolidDuration = 3f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _fadeAlpha = 0.3f;
    private Color _originalColor;
    private Color _fadeColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        if(_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
            _fadeColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, _fadeAlpha);
        }
    }

    private void Start()
    {
        _collider.isTrigger = true;
        _spriteRenderer.color = _fadeColor;
    }
    public void ReactToLightPulse()
    {
        _spriteRenderer.DOFade(1f, _fadeDuration).SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
           _collider.isTrigger = false; 
            StartCoroutine(HandleRemainSolid());
        });
    }

    private IEnumerator HandleRemainSolid()
    {
        yield return new WaitForSeconds(_remainSolidDuration);
        _spriteRenderer.DOFade(_fadeAlpha, _fadeDuration).SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
            _spriteRenderer.color = _fadeColor;
            _collider.isTrigger = true;
        });
    }

}
