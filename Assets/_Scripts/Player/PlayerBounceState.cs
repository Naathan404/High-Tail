using UnityEngine;

public class PlayerBounceState : PlayerState
{
    public PlayerBounceState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.Visual.Anim.Play("pJump");
    }

    public override void LogicUpdate()
    {
        base.PhysicsUpdate();
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
