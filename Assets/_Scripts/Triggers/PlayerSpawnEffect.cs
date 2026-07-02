using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class PlayerSpawnEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private ParticleSystem _gatherParticles;

    [SerializeField] private Light2D _spawnLight;                  // optional, light burst tại vị trí spawn

    [Header("Timing")]
    [SerializeField] private float _delayAfterSceneLoad = 0.1f;   // đợi SceneLoader fade xong rồi mới bắt đầu
    [SerializeField] private float _gatherDuration = 0.8f;        // particle hội tụ
    [SerializeField] private float _fadeInDuration = 0.35f;       // nhân vật fade in
    [SerializeField] private float _landingPunchDuration = 0.2f;  // nảy nhẹ

    [Header("Feel")]
    [SerializeField] private float _landingPunchStrength = 0.12f;
    [SerializeField] private float _spawnLightMaxIntensity = 2f;

    private PlayerController _player;
    private Rigidbody2D _rb;

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
        if (_player == null) return;

        if (_playerSprite == null)
            _playerSprite = FindAnyObjectByType<PlayerVisual>().GetComponent<SpriteRenderer>();

        if (_playerSprite != null)
        {
            Color c = _playerSprite.color;
            _playerSprite.color = new Color(c.r, c.g, c.b, 0f);
        }

        if (_rb == null)
            _rb = _player.GetComponent<Rigidbody2D>();

        InputManager.Instance.EnableControl();
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        yield return new WaitForSeconds(_delayAfterSceneLoad);

        // ── NHỊP 2: Particle hội tụ ───────────────────────────────────────────
        if (_gatherParticles != null)
        {
            _gatherParticles.transform.position = _player.transform.position;
            _gatherParticles.Play();
        }

        // Light burst nhỏ rồi tắt dần
        if (_spawnLight != null)
        {
            _spawnLight.intensity = _spawnLightMaxIntensity;
            DOTween.To(() => _spawnLight.intensity, x => _spawnLight.intensity = x,
                0f, _gatherDuration * 2).SetEase(Ease.InQuad);
        }

        yield return new WaitForSeconds(_gatherDuration);

        // ── NHỊP 3: Nhân vật fade in ──────────────────────────────────────────
        if (_playerSprite != null)
            _playerSprite.DOFade(1f, _fadeInDuration);

        yield return new WaitForSeconds(_fadeInDuration);

        if (_playerSprite != null)
        {
            Color c = _playerSprite.color;
            _playerSprite.color = new Color(c.r, c.g, c.b, 1f);
        }

        // ── NHỊP 4: Landing punch ─────────────────────────────────────────────
        _player.transform.DOPunchScale(
            Vector3.one * _landingPunchStrength,
            _landingPunchDuration,
            vibrato: 5,
            elasticity: 0.5f);

        // Trả lại quyền điều khiển cho người chơi
        InputManager.Instance.EnableControl();
        if (_rb != null)
            _rb.gravityScale = _player.BaseGravity;
    }
}