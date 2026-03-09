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
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }

        if(_player.DashPressed && _player.CanDash && _player.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
        }

        if (Mathf.Abs(_player.MoveX) > 0.01f)
        {
            _stateMachine.ChangeState(_player.RunState);
        }

        if(_player.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.JumpPressed)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
        }
        if(_player.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.SlideGlideHeld)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
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
