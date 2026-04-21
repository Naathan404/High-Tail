using UnityEngine;
using DG.Tweening; // Nhớ cài đặt DOTween
using System.Collections;
using System;

public class AstralGift : MonoBehaviour
{
    [Header("Hiệu ứng thị giác")]
    [SerializeField] private ParticleSystem _collectParticles; 
    [SerializeField] private float _hitStopDuration = 0.1f; 
    [SerializeField] private float _scaleDuration = 0.3f; 
    [SerializeField] private float _maxScale = 1.5f; 
    [SerializeField] private float _floatingDuration = 1f;
    [SerializeField] private float _floatingOffsetY = 1f;

    [Header("Cài đặt khác")]
    private bool _isCollected = false;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private float _startPosY;

    public static event Action<Transform> OnCollected;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _startPosY = transform.position.y;
        transform.DOMoveY(_startPosY + _floatingOffsetY, _floatingDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu chạm vào Tale và chưa được nhặt
        if (collision.CompareTag("Player") && !_isCollected)
        {
            Collect();
        }
    }

    private void Collect()
    {
        _isCollected = true;
        _collider.enabled = false; 

        OnCollected?.Invoke(transform);
        StartCoroutine(HandleCollect(_hitStopDuration));


    }

    private IEnumerator HandleCollect(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; 
        
        yield return new WaitForSecondsRealtime(duration);
        
        Time.timeScale = originalTimeScale; 

        // play particle fx
        if (_collectParticles != null)
        {
            _collectParticles.Play();
            CameraShakeManager.Instance.ShakeForDash();
        }

        // play anim 
        Sequence collectSequence = DOTween.Sequence();
        collectSequence.Append(transform.DOScale(_maxScale, _scaleDuration * 0.4f).SetEase(Ease.OutBack));
        collectSequence.Append(transform.DOScale(0f, _scaleDuration * 0.6f).SetEase(Ease.InBack));
        collectSequence.OnComplete(() => {
            // PlayerController.Instance.AddScore(1); 
            Destroy(gameObject, 0.5f);
        });        
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}