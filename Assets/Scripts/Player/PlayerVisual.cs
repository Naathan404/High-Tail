using System;
using DG.Tweening;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Vector3 _originalScale;
    public Animator Anim { get; private set; }
    [SerializeField] private float squashStretchDuration;
    public EffectPooler JumpDustPool;
    public EffectPooler FallDustPool;
    public ParticleSystem RunDustParticle;
    public ParticleSystem DashDustParticle;
    public ParticleSystem SlideDustParticle;
    public ParticleSystem JumpDustParticle;
    private void Awake()
    {
        Anim = GetComponent<Animator>();
    }

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
