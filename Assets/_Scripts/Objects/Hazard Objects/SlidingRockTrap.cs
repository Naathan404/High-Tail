using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class SlidingRockTrap : MonoBehaviour
{
    [SerializeField] private Transform _rockTransform; 
    [SerializeField] private Transform _endPoint; 
    private Vector2 _originalRockPosition;
    private BoxCollider2D _collider;
    [SerializeField] private CrusherDamage _killZone;
    
    [Header("Effects")]
    [SerializeField] private float _warningTime = 0.2f;
    [SerializeField] private int _vibrato = 15;
    [SerializeField] private float _strength = 0.2f; 
    [SerializeField] private float _slideSpeed = 0.15f;
    [SerializeField] private float _shakeForceMultiplier = 1f;
    [SerializeField] private ParticleSystem _collisonParticle;
    
    private bool _hasTriggered = false;


    private void Start()
    {
        _collider = _rockTransform.GetComponent<BoxCollider2D>();
        _collider.enabled = true;
        _originalRockPosition = _rockTransform.position;
        _killZone.IsDangerous = false;
    }

    private void OnEnable()
    {
        DeathScreenManager.OnDeathScreenTriggered += Reset;
    }

    private void OnDisable()
    {
        DeathScreenManager.OnDeathScreenTriggered -= Reset;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;
            DropTheRock();
        }
    }

    private void DropTheRock()
    {
        _rockTransform.DOShakePosition(duration: _warningTime, strength: _strength, vibrato: _vibrato)
            .OnComplete(() => 
            {
                _killZone.IsDangerous = true;
                _collider.enabled = false;
                _rockTransform.DOMoveX(_endPoint.position.x, _slideSpeed)
                    .SetEase(Ease.InExpo)
                    .SetUpdate(UpdateType.Fixed)
                    .OnComplete(() => 
                    {
                        CameraShakeManager.Instance.ShakeCustom(_shakeForceMultiplier);
                        _collisonParticle.gameObject.SetActive(true);
                        _collisonParticle.Play();
                        _collider.enabled = true;
                        _killZone.IsDangerous = false;
                    });
            });
    }

    private void Reset()
    {
        _hasTriggered = false;
        _rockTransform.position = _originalRockPosition;
        _collider.enabled = true;
        _killZone.IsDangerous = false;
    }
}