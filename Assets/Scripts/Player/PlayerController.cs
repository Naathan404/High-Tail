using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData Data;
    private PlayerStateMachine _stateMachine;

    [Header("Components")] //==========================================================
    public Animator Anim;
    public Rigidbody2D Rb;
    private PlayerControls _controls;

    [Header("Player States")] //==========================================================
    public PlayerIdleState IdleState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }

    [Header("Player Inputs")] //==========================================================
    public float MoveX { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool DashPressed { get; private set; }

    [Header("Player Variables")]
    [SerializeField] private float _jumpBufferCounter;
    [SerializeField] private float _coyoteCounter;
    [SerializeField] private bool _isFacingRight;
    [SerializeField] private bool _isGround;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _castDistance;
    [SerializeField] private Vector2 _footSize;
    [SerializeField] private Vector3 _footPosition;


    private void Awake()
    {
        _stateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, _stateMachine);
        RunState = new PlayerRunState(this, _stateMachine);
        JumpState = new PlayerJumpState(this, _stateMachine);
        DashState = new PlayerDashState(this, _stateMachine);
        FallState = new PlayerFallState(this, _stateMachine);

        _controls = new PlayerControls();
    }

    private void Start()
    {
        _stateMachine.InitStateMachine(IdleState);

        _isFacingRight = true;
    }

    private void OnEnable()
    {
        _controls.Movement.Enable();
    }

    private void OnDisable()
    {
        _controls.Movement.Disable();
    }

    private void Update()
    {
        // ground check
        _isGround = GroundCheck();

        // Lấy input từ bàn phím
        MoveX = _controls.Movement.Move.ReadValue<Vector2>().x;
        JumpPressed = _controls.Movement.Jump.WasPressedThisFrame();
        JumpHeld = _controls.Movement.Jump.IsPressed();
        //DashPressed = Input.GetKeyDown(KeyCode.Z);

        // Check điều kiện nhảy
        if(_isGround)
            _coyoteCounter = Data.coyoteTime;
        else
            _coyoteCounter -= Time.deltaTime;
        if(JumpPressed)
            _jumpBufferCounter = Data.jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;
        
        
        // Xử lý ở state hiện tại
        _stateMachine.CurrentState.HandleInput();
        _stateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        _stateMachine.CurrentState.PhysicsUpdate();
    }

    public void HandleHorizontalMovement()
    {
        float targetSpeed = MoveX * Data.maxMoveSpeed;
        // nếu có nhấn nút thì dùng acceleration, nếu buông nút thì dùng decceleration
        float accelerationRate = (Mathf.Abs(MoveX) > 0.01f) ? Data.acceleration : Data.decceleration;
        float newVelocityX = Mathf.MoveTowards(Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);

        Rb.linearVelocity = new Vector2(newVelocityX, Rb.linearVelocity.y);
    }

    public bool CanJump() => _coyoteCounter > 0 && _jumpBufferCounter > 0;
    public void UseJumpBuffer() => _jumpBufferCounter = 0;
    public void SetJumpBufferTimer() => _jumpBufferCounter = Data.jumpBufferTime;
    public bool IsOnGround() => _isGround;

    public bool GroundCheck()
    {
        RaycastHit2D hit = Physics2D.BoxCast
        (
            this.transform.position + _footPosition, 
            _footSize, 
            0f, 
            Vector2.down, 
            _castDistance, 
            _groundLayerMask
        );

        return hit.collider != null;
    }

    public void CheckIfShoundFlip(float moveX)
    {
        Vector2 scale = this.transform.localScale;
        if(moveX > 0 && !_isFacingRight)
        {
            _isFacingRight = !_isFacingRight;
            this.transform.localScale = new Vector2(Mathf.Abs(scale.x), scale.y);
        }
        else if(moveX < 0 && _isFacingRight)
        {
            _isFacingRight = !_isFacingRight;
            this.transform.localScale = new Vector2(Mathf.Abs(scale.x) * -1f, scale.y);            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isGround ? Color.green : Color.red;
        Gizmos.DrawWireCube(this.transform.position + _footPosition + (Vector3.down * _castDistance), _footSize);
    }
}
