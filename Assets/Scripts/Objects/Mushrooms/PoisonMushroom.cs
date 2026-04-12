using UnityEngine;
using System.Collections;

public class PoisonMushroom : MonoBehaviour
{
    [Header("Poison Settings")]
    [SerializeField] private float _poisonSpeed = 20f; // Tốc độ bay của khói
    [SerializeField] private float _fireRate = 50f; // Tốc độ bắn (Số hạt sinh ra mỗi giây)
    [SerializeField] private float _bulletLifetime = 1.5f; // Thời gian hủy đạn (Khói tồn tại bao lâu)
    [SerializeField] private PoisonDirection _poisonDirection = PoisonDirection.Up;

    [Header("Noise Settings")]
    [SerializeField] private bool _useNoise = true; // Checkbox để bật/tắt nhanh hiệu ứng
    [SerializeField] private float _noiseStrength = 1.2f; // Độ rung lắc
    [SerializeField] private float _noiseFrequency = 1.5f;

    [Header("Timers")]
    [SerializeField] private float _spewTime = 5f;
    [SerializeField] private float _cooldownTime = 2f;

    [Header("References")]
    [SerializeField] private ParticleSystem _poisonParticles;

    [Header("Detector")]
    [SerializeField] private float _detectRadius = 5f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Damage Settings")]
    [SerializeField] private int _poisonDamage = 1;
    public int PoisonDamage => _poisonDamage;

    private bool _isSpewing = false;

    private enum PoisonDirection
    {
        Up,
        Left,
        Right
    }

    private void Start()
    {
        if (_poisonParticles == null)
        {
            _poisonParticles = GetComponentInChildren<ParticleSystem>();
        }
        ApplyParticleSettings();
    }

    private void Update()
    {
        CheckPlayerProximity();
    }
    private void ApplyParticleSettings()
    {
        if (_poisonParticles == null) return;

        var mainModule = _poisonParticles.main;
        mainModule.startSpeed = _poisonSpeed;
        mainModule.startLifetime = _bulletLifetime;

        var emissionModule = _poisonParticles.emission;
        emissionModule.rateOverTime = _fireRate;

        var noiseModule = _poisonParticles.noise;
        noiseModule.enabled = _useNoise;
        if (_useNoise)
        {
            noiseModule.strength = _noiseStrength;
            noiseModule.frequency = _noiseFrequency;
        }

        Vector3 rotationAngle = Vector3.zero;
        switch (_poisonDirection)
        {
            case PoisonDirection.Up:
                rotationAngle = new Vector3(-90f, 0f, 0f); 
                break;
            case PoisonDirection.Left:
                rotationAngle = new Vector3(0f, -90f, 0f); 
                break;
            case PoisonDirection.Right:
                rotationAngle = new Vector3(0f, 90f, 0f); 
                break;
        }
        _poisonParticles.transform.localEulerAngles = rotationAngle;
    }

    private void CheckPlayerProximity()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, _detectRadius, _playerLayer);

        if (playerHit != null && !_isSpewing)
        {
            StartCoroutine(SpewPoisonRoutine());
        }
    }

    private IEnumerator SpewPoisonRoutine()
    {
        _isSpewing = true;

        _poisonParticles.Play();

        yield return new WaitForSeconds(_spewTime);

        _poisonParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        yield return new WaitForSeconds(_cooldownTime);

        _isSpewing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
    }
}