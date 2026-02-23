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
    public bool DashPressed { get; private set; }

    [Header("Player Variables")]
    [SerializeField] private bool _isGround;
    [SerializeField] private float _jumpBufferCounter;
    [SerializeField] private float _coyoteCounter;
    [SerializeField] private bool _isFacingRight;

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
        // Lấy input từ bàn phím
        MoveX = _controls.Movement.Move.ReadValue<Vector2>().x;
        JumpPressed = _controls.Movement.Jump.WasPressedThisFrame();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            _isGround = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            _isGround = false;
        }
    }

    public void CheckIfShoundFlip(float moveX)
    {
        if(moveX > 0 && !_isFacingRight)
        {
            _isFacingRight = !_isFacingRight;
            this.transform.localScale = new Vector2(1f, 1f);
        }
        else if(moveX < 0 && _isFacingRight)
        {
            _isFacingRight = !_isFacingRight;
            this.transform.localScale = new Vector2(-1f, 1f);            
        }
    }

}
