using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào run state");
        _player.CanDash = true;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        _player.CheckFlip(_player.MoveX);

        if(Mathf.Abs(_player.MoveX) < 0.01f && Mathf.Abs(_player.Rb.linearVelocity.x) < 0.1f)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(_player.DashPressed && _player.CanDash && _player.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
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

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleHorizontalMovement();
    }    
}
