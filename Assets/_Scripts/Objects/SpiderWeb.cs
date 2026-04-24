using UnityEngine;
using UnityEngine.UI;

public class SpiderWeb : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private Slider _slider;
    [SerializeField] private GameObject _visual;
    [SerializeField] private float _maxDuration = 3f; // Thời gian phá bẫy
    [SerializeField] private float _respawnTime = 5f; // Thời gian bẫy hiện lại

    private float _timer;
    private float _hiddenTimer;
    private bool _isStickPlayer = false;
    private bool _isCooldown = false;
    private PlayerController _player;

    private void Start()
    {
        _slider.gameObject.SetActive(false);
        _timer = _maxDuration;
    }

    private void Update()
    {
        if (_isStickPlayer && _player != null)
        {
            _timer -= Time.deltaTime;
            _slider.value = _timer / _maxDuration;

            if (_timer <= 0)
            {
                ReleasePlayer();
            }
        }

        if (_isCooldown)
        {
            _hiddenTimer -= Time.deltaTime;
            if (_hiddenTimer <= 0)
            {
                ResetTrap();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isCooldown || _isStickPlayer) return;

        if (collision.CompareTag("Player"))
        {
            _player = collision.GetComponent<PlayerController>();
            if (_player != null)
            {
                _player.IsBlocked = true; 
                _isStickPlayer = true;
                _timer = _maxDuration; 
                
                _slider.gameObject.SetActive(true);
                _slider.value = 1f;
            }
        }
    }

    private void ReleasePlayer()
    {
        if (_player != null) _player.IsBlocked = false;
        
        _isStickPlayer = false;
        _isCooldown = true;
        _hiddenTimer = _respawnTime; 

        _visual.SetActive(false); 
        _slider.gameObject.SetActive(false);
    }

    private void ResetTrap()
    {
        _isCooldown = false;
        _visual.SetActive(true); 
        _timer = _maxDuration;
    }
}