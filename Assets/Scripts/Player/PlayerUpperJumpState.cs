using UnityEngine;

public class PlayerUpperJumpState : PlayerState
{
    private float jumpForce;
    public PlayerUpperJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào upper jump state");
        _player.UseJumpBuffer();
        _player.Visual.ApplySquashStretch(new Vector3(0.8f, 1.3f, 1f));
        
        if (_player.IsStickyGround) 
            jumpForce = _player.Data.upperJumpForce * 0.5f;
        else 
            jumpForce = _player.Data.upperJumpForce;

        // _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, _player.Data.jumpForce);
        //_player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x * boost, 0);
        _player.Rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        GameObject obj = _player.Visual.JumpDustPool.GetObject();
        obj.transform.position = _player.Visual.transform.position;
        _player.Visual.JumpDustPool.ReturnToPool(obj);
        _player.Visual.Anim.Play("playerJump");
        _player.Visual.JumpDustParticle.Play();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (_player.MoveY < -0.5f && !_player.IsOnGround()) 
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
        // if(_player.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.SlideGlideHeld && !_player.IsSlipWall)
        // {
        //     _stateMachine.ChangeState(_player.WallSlideState);
        //     return;
        // }               
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
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
    }    
}
