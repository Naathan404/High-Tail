using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class TriggerPlatform : MonoBehaviour
{
    public enum TriggerType { Top, Side, Both }
    public enum SideAttachMode { Stick, Slide }

    [Header("Settings")]
    [SerializeField] private TriggerType _triggerType = TriggerType.Top;
    [SerializeField] private SideAttachMode _sideMode = SideAttachMode.Stick;
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _returnSpeed = 15f;

    private Vector2 _startPosition;
    private bool _isMoving = false;
    private bool _hasReachedTarget = false;
    private Rigidbody2D _rb;
    private PlayerController _player;
    public Vector3 DeltaPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _startPosition = transform.position;

        DeltaPos = Vector2.zero;
    }

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
        if(_player == null) Debug.Log("Khong tim thay nguoi choi");
        if (_player != null) _player.OnPlayerDied += ResetPlatform;
    }

    private void OnDisable()
    {
        if (_player != null) _player.OnPlayerDied -= ResetPlatform;
    }

    private void FixedUpdate()
    {
        if (_isMoving && !_hasReachedTarget)
        {
            MoveTowards(_targetPoint.position, _moveSpeed);
            if (Vector2.Distance(transform.position, _targetPoint.position) < 0.05f)
            {
                _hasReachedTarget = true;
                _isMoving = false;
                // Bục tới đích thì dừng lại bằng MovePosition nên không cần ép velocity về 0 nữa
            }
        }
        else if(!_isMoving)
        {
            _rb.linearVelocity = Vector2.zero;
            DeltaPos = Vector2.zero;
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        // THAY ĐỔI LỚN: Dùng MovePosition thay cho linearVelocity
        // Điều này giúp bục "cõng" Sóc Kite (làm con) cực kỳ mượt mà không bị rung giật
        Vector2 newPos = Vector2.MoveTowards(_rb.position, target, speed * Time.fixedDeltaTime);
        DeltaPos = newPos - _rb.position;
        _rb.MovePosition(newPos);
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        _isMoving = false;
        _hasReachedTarget = false;

        while (Vector2.Distance(transform.position, _startPosition) > 0.05f)
        {
            MoveTowards(_startPosition, _returnSpeed);
            yield return new WaitForFixedUpdate();
        }

        _rb.MovePosition(_startPosition);
        DeltaPos = Vector2.zero;
    }

    private void HandleCollision(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _player == null) return;

        ContactPoint2D contact = collision.GetContact(0);
        bool hitTop = contact.normal.y < -0.5f;
        bool hitSide = Mathf.Abs(contact.normal.x) > 0.5f;

        // Kích hoạt bục chạy
        if ((_triggerType == TriggerType.Top && hitTop) || 
            (_triggerType == TriggerType.Side && hitSide && _player.StateMachine.CurrentState == _player.WallSlideState) || 
            (_triggerType == TriggerType.Both))
        {
            _isMoving = true;
        }

        // Xử lý SetParent
        bool shouldStick = hitTop || (_triggerType == TriggerType.Side && _sideMode == SideAttachMode.Stick && hitSide);
        
        if (shouldStick)
        {
            if (collision.transform.parent != transform)
            {
                collision.transform.SetParent(transform, true);
            }
        }
        else
        {
            if (collision.transform.parent == transform)
            {
                _player.ReturnToCoreScene(); 
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // _isMoving = false;
            // DeltaPos = Vector2.zero;
            if (collision.transform.parent == transform)
            {
                collision.gameObject.GetComponent<PlayerController>()?.ReturnToCoreScene();
            }
        }
    }
}