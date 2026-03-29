using System;
using DG.Tweening;
using UnityEngine;

public class PushMushroom : MonoBehaviour, IPushable
{
    [SerializeField] private Transform _visual;
    
    // Đổi Force thành Vector2 để có lực nẩy chéo
    [Header("Push Force (X: Ngang, Y: Nảy lên)")]
    [SerializeField] private Vector2 _pushForce = new Vector2(20f, 10f); 
    
    [SerializeField] private bool _isFacingRight;
    
    public Vector2 PushForce => new Vector2(_pushForce.x * IsRight, _pushForce.y); 
    
    private Vector3 _originalScale;
    public float IsRight => _isFacingRight ? 1f : -1f;

    private void Start()
    {
        _visual.transform.localScale = new Vector3(_visual.transform.localScale.x * IsRight, _visual.transform.localScale.y, 1f);
        _originalScale = _visual.transform.localScale;
    }

    public void PlayPushAnimation()
    {
        _visual.transform.DOKill();
        _visual.transform.DOScale(new Vector3(1.5f * IsRight, 0.6f), 0.1f)
               .OnComplete(() => _visual.transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic));
    }
}