using System;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData Data;
    private PlayerStateMachine _stateMachine;

    [Header("Components")] //==========================================================
    public Animator Anim;
    public Rigidbody2D Rb;
    private PlayerControls _controls;
    public PlayerVisual Visual;

    [Header("Player States")] //==========================================================
    public PlayerIdleState IdleState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerAirGlideState AirGlideState { get; private set; }

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
    public bool CanDash = true;

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
        WallSlideState = new PlayerWallSlideState(this, _stateMachine);
        AirGlideState = new PlayerAirGlideState(this, _stateMachine);

        _controls = new PlayerControls();
    }

    private void Start()
    {
        _stateMachine.InitStateMachine(IdleState);

        _isFacingRight = true;
        Rb.gravityScale = Data.gravityScale * Data.fallMultiplier;
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
        // ground check liên tục mỗi frame
        _isGround = GroundCheck();

        // Lấy input từ bàn phím
        MoveX = _controls.Movement.Move.ReadValue<Vector2>().x;         // di chuyen trai phai
        JumpPressed = _controls.Movement.Jump.WasPressedThisFrame();    // nhay
        JumpHeld = _controls.Movement.Jump.IsPressed();                 // nhay cao hon khi giu lau
        DashPressed = _controls.Movement.Dash.WasPressedThisFrame();    // dash

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
        // ***NOTE***: Mathf.MoveToward(current, target, maxDelta) để di chuyển giá trị từ current đến target
        // theo một khoảng giá trị tối đa maxDelta có thể thay đổi trong một khung hình --> Tốc độ * deltaTime
        float newVelocityX = Mathf.MoveTowards(Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);

        Rb.linearVelocity = new Vector2(newVelocityX, Rb.linearVelocity.y);
    }

    public void HandleAirMovement()
    {
        // di chuyển ngang khi giữ nút di chuyển
        float targetSpeed = MoveX * Data.maxMoveSpeed;
        float accelerationRate = Mathf.Abs(MoveX) > 0.1f ? Data.acceleration : 0f;
        float newVelocityX = Mathf.MoveTowards(Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        
        Rb.linearVelocity = new Vector2(newVelocityX, Rb.linearVelocity.y);
    }

    public bool CanJump() => _coyoteCounter > 0 && _jumpBufferCounter > 0;
    public void UseJumpBuffer() => _jumpBufferCounter = 0;
    public void SetJumpBufferTimer() => _jumpBufferCounter = Data.jumpBufferTime;
    public bool IsOnGround() => _isGround;
    public bool IsFacingRight() => _isFacingRight;

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

    public void CheckFlip(float moveX)
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

    // Vẽ gizmos ra scene
    private void OnDrawGizmos()
    {
        Gizmos.color = _isGround ? Color.green : Color.red;
        Gizmos.DrawWireCube(this.transform.position + _footPosition + (Vector3.down * _castDistance), _footSize);
    }
}
