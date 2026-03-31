using DG.Tweening;
using UnityEngine;

// The code is kinda dirty but still working properly, im gonna refactor it later

public class FloatingPlatform : MonoBehaviour
{
    [SerializeField] private float _interval; // The time when platform start to move until it return to original position
    [Tooltip("Factor * player's velocity -> force when hit platform")]
    [SerializeField] private float _factor;
    [SerializeField] private float _maxForce = 3f;
    [SerializeField] private Rigidbody2D _playerRb;

    private bool _isMoving;
    private float _maxHeight;
    private Vector3 _forceVector;

    private void Start()
    {
        _maxHeight = transform.position.y;
    }

    private void Update()
    {
        float height = _playerRb.transform.position.y;
        if (_maxHeight < height)
        {
            _maxHeight = height;
        }
    }

    private void Bound()
    {
        if (_isMoving) return;

        _isMoving = true;
        // Move the platform to the target position
        this.transform.DOPunchPosition(_forceVector, _interval, 1, 0f, false)
                    .SetEase(Ease.OutExpo)
                    .OnComplete(() => _isMoving = false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            float force = Mathf.Abs(_playerRb.transform.position.y - _maxHeight);
            force = Mathf.Clamp(force * _factor, 0, _maxForce);
            _forceVector = Vector3.down * force;
            Bound();
            _maxHeight = transform.position.y;
        }
    }
}
