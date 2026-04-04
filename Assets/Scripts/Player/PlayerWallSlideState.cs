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

        _player.CanDash = true;
        _originalGrivityScale = _player.Rb.gravityScale;
        _player.Rb.gravityScale = _player.Data.wallSlideGravityScale;
        _player.Rb.linearVelocity = Vector2.zero;
        _player.Visual.SlideDustParticle.Play();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!_player.GrabHeld)
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }

        if(_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.IdleState);
            return;
        }

        if (!_player.IsTouchingWall())
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }

        if (_player.JumpPressed && _player.WallJumpUnlocked)
        {
            bool isFacingRight = _player.IsFacingRight();
            bool isHoldingUp = _player.MoveY > 0.5f; 
            bool isHoldingTowardsWall = (isFacingRight && _player.MoveX > 0.1f) || (!isFacingRight && _player.MoveX < -0.1f);
            bool isNoHorizontalInput = Mathf.Abs(_player.MoveX) <= 0.1f;

            if (isHoldingUp && (isHoldingTowardsWall || isNoHorizontalInput))
            {
                _stateMachine.ChangeState(_player.UpperJumpState);
            }
            else
            {
                _stateMachine.ChangeState(_player.WallJumpState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _originalGrivityScale;
        _player.Visual.SlideDustParticle.Stop();
    }
}
