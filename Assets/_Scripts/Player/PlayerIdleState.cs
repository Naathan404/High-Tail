using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào idle state");
        _player.CanDash = true;
        if(_player.IsStickyGround) _player.CanDash = false;
        
        _player.Visual.Anim.Play("pIdle");
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if(!_player.IsOnGround() && !_player.IsStickyGround)
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
            return;
        }

        if(_player.DashPressed && _player.CanDash && _player.Data.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
            return;
        }

        if (Mathf.Abs(_player.MoveX) > 0.01f)
        {
            _stateMachine.ChangeState(_player.RunState);
            return;
        }

        if(_player.Data.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.JumpPressed)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
            return;
        }
        if(_player.Data.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.GrabHeld)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
            return;
        }        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _player.CheckFlip(_player.MoveX);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleHorizontalMovement();
    }
}
