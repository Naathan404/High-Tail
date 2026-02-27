using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        float jumpDirection = _player.IsFacingRight() ? -1f : 1f;
        Vector2 jumpForce = new Vector2(jumpDirection * _player.Data.jumpWallForce, _player.Data.jumpForce);

        _player.Rb.linearVelocity = Vector2.zero;
        _player.Rb.AddForce(jumpForce, ForceMode2D.Impulse);

        _player.CheckFlip(jumpDirection);
        _player.Visual.ApplySquashStretch(new Vector3(0.7f, 1.3f, 1f));
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(_player.Rb.linearVelocity.y < 0f)
        {
            _stateMachine.ChangeState(_player.FallState);
        }
        if(_player.DashPressed && _player.CanDash && _player.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
        }
    }
}
