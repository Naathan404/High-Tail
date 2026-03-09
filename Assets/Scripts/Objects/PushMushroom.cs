using System;
using DG.Tweening;
using UnityEngine;

public class PushMushroom : MonoBehaviour, IPushable
{
    [SerializeField] private Transform _visual;
    [SerializeField] private float _bouncyForce;
    [SerializeField] private bool _isFacingRight;
    public float PushForce => _bouncyForce;
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
        _visual.transform.DOScale(new Vector3(0.6f * IsRight, 1.5f), 0.1f);
        _visual.transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic);
    }
}
