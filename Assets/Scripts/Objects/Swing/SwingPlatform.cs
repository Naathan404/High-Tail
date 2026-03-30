using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwingPlatform : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Rigidbody2D _playerRb;

    [Header("Physics Settings")]
    [Tooltip("Hệ số nhân lực đạp của Player lên xích đu. Tăng số này nếu muốn ván lắc mạnh hơn khi chạy.")]
    public float kickForceMultiplier = 50f;

    // Biến DeltaPos để script Player của em đọc (giống hệt bên MovingPlatform)
    public Vector2 DeltaPos { get; private set; }
    private Vector2 _lastPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _lastPosition = _rb.position;
    }

    private void FixedUpdate()
    {
        // 1. TÍNH ĐỘ DỜI (DELTAPOS) ĐỂ PLAYER BÁM THEO
        DeltaPos = _rb.position - _lastPosition;
        _lastPosition = _rb.position;

        // 2. TẠO LỰC ĐẠP VẬT LÝ KHI PLAYER DI CHUYỂN
        if (_playerRb != null)
        {
            float playerMoveX = _playerRb.linearVelocity.x;

            // Kiểm tra xem Player có đang thực sự di chuyển ngang không
            if (Mathf.Abs(playerMoveX) > 0.1f)
            {
                // Áp dụng Định luật 3 Newton: Player chạy tới -> đẩy ván về sau
                // Lực được AddForceAtPosition vào đúng chỗ Player đứng để tạo độ nghiêng tự nhiên
                Vector2 kickForce = new Vector2(-playerMoveX * kickForceMultiplier * Time.fixedDeltaTime, 0);
                _rb.AddForceAtPosition(kickForce, _playerRb.position, ForceMode2D.Impulse);
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
        }
    }
}