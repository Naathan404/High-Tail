using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float _fallDelay;
    [SerializeField] private float _resetDelay; // The time it takes for the platform to reset after stop at the stop position
    [SerializeField] private float _resetDuration;
    [SerializeField] private float _fallDistance; // The distance the platform will fall before stopping

    private Trigger _trigger;
    private Vector3 _startPosition;
    private Vector3 _stopPosition; // The position where the platform will stop after falling
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private void Awake()
    {
        _startPosition = transform.position;
        _stopPosition = _startPosition - Vector3.up * _fallDistance;
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Kinematic; // Start as kinematic to prevent falling immediately
        _collider = GetComponent<Collider2D>();
        _trigger = GetComponentInChildren<Trigger>();
    }

    private void OnEnable()
    {
        _trigger.OnStepIn += FallAction;
    }

    private void OnDisable()
    {
        _trigger.OnStepIn -= FallAction;
    }

    private void FallAction()
    {
        StartCoroutine(FallCoroutine());
    }

    private IEnumerator FallCoroutine()
    {
        yield return new WaitForSeconds(_fallDelay);
        Fall();
        yield return new WaitUntil(() => (transform.position.y <= _stopPosition.y));
        Stop();
    }

    private void Fall()
    {
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _collider.enabled = false;
    }

    private void Stop()
    {
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.linearVelocity = Vector2.zero;
        StartCoroutine(ResetPosition());
    }

    private IEnumerator ResetPosition()
    {
        yield return new WaitForSeconds(_resetDelay);
        // Move to the start position
        if (_stopPosition == null)
            yield return null;
        transform.DOMove(_startPosition, _resetDuration).SetEase(Ease.InOutSine);

        yield return new WaitUntil(() => (transform.position == _startPosition));
        _collider.enabled = true; // Bật lại collider sau khi đã di chuyển về vị trí ban đầu
    }
}
