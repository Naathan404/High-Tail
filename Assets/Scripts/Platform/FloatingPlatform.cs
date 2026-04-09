using DG.Tweening;
using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    [Header("Juice Settings")]
    [Tooltip("Duration)")]
    [SerializeField] private float _dipDuration = 0.3f;
    [Tooltip("Max Dip Distance")]
    [SerializeField] private float _maxDipDistance = 0.8f;
    [SerializeField] private float _forceMultiplier = 0.05f;

    private Vector3 _startPosition;
    private Tween _bounceTween;
    private bool _isBouncing = false;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y < -0.5f)
            {
                float fallSpeed = Mathf.Abs(collision.relativeVelocity.y);
                float dipAmount = Mathf.Clamp(fallSpeed * _forceMultiplier, 0.1f, _maxDipDistance);
                
                if(_isBouncing) return;
                collision.transform.SetParent(this.transform);
                Dip(dipAmount, collision.gameObject);
            }
        }
    }

    private void Dip(float dipAmount, GameObject player)
    {
        _bounceTween?.Kill(true);
        _isBouncing = true;

        // bounce effect
        _bounceTween = transform.DOPunchPosition(Vector3.down * dipAmount, _dipDuration, 1, 0f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                // return to the origin pos for sure
                transform.position = _startPosition;
                _isBouncing = false;
                player.transform.SetParent(null);
            });
    }

    private void OnDisable()
    {
        _bounceTween?.Kill();
        transform.position = _startPosition;
        _isBouncing = false;
    }
}