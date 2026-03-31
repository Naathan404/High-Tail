using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwingPlatform : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Rigidbody2D _playerRb;

    [Header("Force")]
    public float pushForce = 5f; // Lực đẩy cùng chiều với Player

    // Biến này để Script Player của em đọc (Giống hệt MovingPlatform)
    [HideInInspector] public Vector2 DeltaPos;
    private Vector2 _lastPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Start()
    {
        _lastPosition = _rb.position;
    }

    private void FixedUpdate()
    {
        DeltaPos = _rb.position - _lastPosition;
        _lastPosition = _rb.position;

        if (_playerRb != null)
        {
            float moveInput = _playerRb.linearVelocity.x;

            if (Mathf.Abs(moveInput) > 0.1f)
            {
                Vector2 force = new Vector2(moveInput * pushForce, 0);
                _rb.AddForce(force);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRb = null;
            DeltaPos = Vector2.zero;
        }
    }
}