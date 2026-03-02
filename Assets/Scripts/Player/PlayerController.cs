using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SearchService;

public partial class PlayerController : MonoBehaviour
{
    public PlayerData Data;
    private PlayerStateMachine _stateMachine;

    [Header("Components")] //==========================================================
    public Animator Anim;
    public Rigidbody2D Rb;
    public PlayerVisual Visual;

    [Header("Player States")] //==========================================================
    public PlayerIdleState IdleState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerAirGlideState AirGlideState { get; private set; }

    [Header("Skills Unlock")]
    public bool WallJumpUnlocked;
    public bool WallSlideUnlocked;
    public bool DashUnlocked;
    public bool AirGlideUnlocked;

    [Header("Player Inputs")] //==========================================================
    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public bool SlideGlideHeld { get; private set; }

    [Header("Player Variables")]
    [SerializeField] private float _jumpBufferCounter;
    [SerializeField] private float _coyoteCounter;
    [SerializeField] private bool _isFacingRight;
    [SerializeField] private bool _isGround;
    public bool CanDash = true;
    public Vector2 DashDirection;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _castDistance;
    [SerializeField] private Vector2 _footSize;
    [SerializeField] private Vector3 _footPosition;

    [Header("Wall Check")]
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private Vector2 _wallCheckSize;
    [SerializeField] private Transform _wallCheck;

    private PlayerControls Inputs => InputManager.Instance.Inputs;
    private void Awake()
    {
        _stateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, _stateMachine);
        RunState = new PlayerRunState(this, _stateMachine);
        JumpState = new PlayerJumpState(this, _stateMachine);
        DashState = new PlayerDashState(this, _stateMachine);
        FallState = new PlayerFallState(this, _stateMachine);
        WallSlideState = new PlayerWallSlideState(this, _stateMachine);
        WallJumpState = new PlayerWallJumpState(this, _stateMachine);
        AirGlideState = new PlayerAirGlideState(this, _stateMachine);
    }

    private void Start()
    {
        _stateMachine.InitStateMachine(IdleState);

        _isFacingRight = true;
        Rb.gravityScale = Data.gravityScale * Data.fallMultiplier;
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        // ground check liên tục mỗi frame
        _isGround = GroundCheck();

        // Lấy input từ bàn phím
        MoveX = Inputs.Movement.Move.ReadValue<Vector2>().x;         // di chuyen trai phai
        MoveY = Inputs.Movement.Move.ReadValue<Vector2>().y;         // lấy input trên dưới để tính hướng dash
        JumpPressed = Inputs.Movement.Jump.WasPressedThisFrame();    // nhay
        JumpHeld = Inputs.Movement.Jump.IsPressed();                 // nhay cao hon khi giu lau
        DashPressed = Inputs.Movement.Dash.WasPressedThisFrame();    // dash
        SlideGlideHeld = Inputs.Movement.SlideGlide.IsPressed();     // giữ nút để trượt tường hoặc thả dù

        // tính toán hướng dash
        // if(new Vector2(MoveX, MoveY).magnitude > 0f)
        // {
        //     DashDirection = new Vector2(MoveX, MoveY).normalized;
        // }
        // else
        // {
        //     DashDirection = _isFacingRight ? Vector2.right : Vector2.left;
        // }

        // Check điều kiện nhảy
        if(_isGround)
            _coyoteCounter = Data.coyoteTime;
        else
            _coyoteCounter -= Time.deltaTime;
        if(JumpPressed)
            _jumpBufferCounter = Data.jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;

        // xử lý ở state hiện tại
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
        if(Mathf.Abs(MoveX) < 0.1f) return;
        // di chuyển ngang khi giữ nút di chuyển
        float targetSpeed = MoveX * Data.maxMoveSpeed;
        float accelerationRate = (Mathf.Abs(MoveX) > 0.1f) ? Data.acceleration : 0f;
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

    public bool IsTouchingWall()
    {
        return Physics2D.OverlapBox(_wallCheck.position, _wallCheckSize, 0, _wallLayerMask);
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

        Gizmos.color = IsTouchingWall() ? Color.green : Color.red;
        Gizmos.DrawWireCube(_wallCheck.transform.position, _wallCheckSize);
    }
}
