using System;
using DG.Tweening;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Vector3 _originalScale;
    [SerializeField] private float squashStretchDuration;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void ApplySquashStretch(Vector3 targetScale, float duration = -1f)
    {
        transform.DOKill();
        transform.localScale = _originalScale;
        float scaleDuration = (duration == -1f) ? squashStretchDuration : duration;
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(targetScale, scaleDuration)).SetEase(Ease.OutQuad);
        s.Append(transform.DOScale(_originalScale, scaleDuration)).SetEase(Ease.OutQuad);
    }
}
