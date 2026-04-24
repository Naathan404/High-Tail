using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerVisual : MonoBehaviour
{
    private Vector3 _originalScale;
    [Header("Components")]
    public Animator Anim { get; private set; }

    [Header("Scale")]
    [SerializeField] private float squashStretchDuration;

    [Header("Particles")]
    public EffectPooler JumpDustPool;
    public EffectPooler FallDustPool;
    public ParticleSystem RunDustParticle;
    public ParticleSystem DashDustParticle;
    public ParticleSystem SlideDustParticle;
    public ParticleSystem JumpDustParticle;
    public ParticleSystem FallDustParticle;
    public ParticleSystem LandingDustParticle;

    [Header("Lighting")]
    [SerializeField] private Light2D _spotLight;

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

    public void ToggleSpotLight(InputAction.CallbackContext context)
    {
        if(!GetComponentInParent<PlayerController>().Data.GlowUnlocked) return;
        _spotLight.gameObject.SetActive(!_spotLight.gameObject.activeInHierarchy);
    }
}
