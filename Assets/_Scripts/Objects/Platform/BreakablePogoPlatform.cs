using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BreakablePogoPlatform : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _breakParticles; 

    private bool _isBroken = false;

    private void Awake()
    {
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        DeathScreenManager.OnDeathScreenTriggered += ResetPlatform;
    }

    private void OnDisable()
    {
        DeathScreenManager.OnDeathScreenTriggered -= ResetPlatform;
    }

    public void Break()
    {
        if (_isBroken) return;

        _isBroken = true;
        _collider.enabled = false;
        _spriteRenderer.enabled = false;

        if (_breakParticles != null)
        {
            _breakParticles.Play();
        }

        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCustom(0.3f);
        }

        // NOTE: chơi âm thanh
        // AudioManager.Instance.PlaySFX(SoundName.Platform_Break);
    }

    private void ResetPlatform()
    {
        _isBroken = false;
        _collider.enabled = true;
        _spriteRenderer.enabled = true;
    }
}