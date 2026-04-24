using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private float jumpForce;
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào jump state");
        _player.UseJumpBuffer();
        _player.Visual.ApplySquashStretch(new Vector3(0.8f, 1.3f, 1f));
        _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
        
        if (_player.IsStickyGround) 
            jumpForce = _player.Data.jumpForce * 0.5f;
        else 
            jumpForce = _player.Data.jumpForce;

        float boost = Mathf.Abs(_player.MoveX) > 0.6f ? _player.Data.boostVelocity : 1f;
        // _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, _player.Data.jumpForce);
        //_player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x * boost, 0);
        //_player.Rb.AddForce(new Vector2(_player.MoveX, 1f) * jumpForce, ForceMode2D.Impulse);
        float horizontalSpeed = _player.Rb.linearVelocity.x * boost;
        _player.Rb.linearVelocity = new Vector2(horizontalSpeed, jumpForce);

        _player.Visual.Anim.Play("playerJump");
        _player.Visual.JumpDustParticle.Play();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_player.MoveY < -0.5f && !_player.IsOnGround() && _player.Data.PogoUnlocked) 
        {
            _stateMachine.ChangeState(_player.PogoState);
            return; 
        }
        _player.CheckFlip(_player.MoveX);
        if(_player.Rb.linearVelocity.y < 0f)
        {
            _stateMachine.ChangeState(_player.FallState);
        }
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(_player.DashPressed && _player.CanDash && _player.Data.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
            return;
        }

        if(_player.Data.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.JumpPressed && !_player.IsSlipWall)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
            return;
        }
        if(_player.Data.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.GrabHeld && !_player.IsSlipWall)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
            return;
        }               
    }    

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        _player.HandleAirMovement();
        // dynamic jump
        if(!_player.JumpHeld && _player.Rb.linearVelocity.y > 0f)
        {
            _player.Rb.linearVelocity += Vector2.up * Physics2D.gravity.y * _player.Data.fallMultiplier * Time.fixedDeltaTime;
        }
        if (Mathf.Abs(_player.Rb.linearVelocity.y) < 1.25f && _player.JumpHeld)
        {
            _player.Rb.gravityScale = _player.Data.gravityScale * 1.0f; 
        }
        else if (_player.Rb.linearVelocity.y < 0)
        {
            // Khi bắt đầu rơi thì tăng trọng lực để cảm giác rơi chắc chắn
            _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
