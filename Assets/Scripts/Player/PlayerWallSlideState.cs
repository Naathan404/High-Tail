using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    private float _originalGrivityScale;
    public PlayerWallSlideState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Đã vào wall slide state");

        _originalGrivityScale = _player.Rb.gravityScale;
        _player.Rb.gravityScale = _player.Data.wallSlideGravityScale;
        _player.Rb.linearVelocity = Vector2.zero;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!_player.SlideGlideHeld)
        {
            _stateMachine.ChangeState(_player.FallState);
        }

        if(_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
        else
        {
            if(_player.IsTouchingWall())
            {
                if(_player.JumpPressed && _player.WallJumpUnlocked)
                {
                    _stateMachine.ChangeState(_player.WallJumpState);
                }
            }
            else
            {
                _stateMachine.ChangeState(_player.FallState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _originalGrivityScale;
    }
}
