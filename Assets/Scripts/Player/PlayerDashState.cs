using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float _dashTimer;
    private float _originalGravity;

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        if(!_player.CanDash) return;
        Debug.Log("<color=yellow> ==> Dashing ==> </Color>");
        _originalGravity = _player.Rb.gravityScale;
        _player.Rb.gravityScale = 1f;

        _dashTimer = _player.Data.dashDuration;
        Vector2 direction = _player.IsFacingRight() ? Vector2.right : Vector2.left;
        _player.Rb.linearVelocity = direction * _player.Data.dashForce; 

        _player.CanDash = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0)
        {
            if (_player.IsOnGround())
                _stateMachine.ChangeState(_player.IdleState);
            else
                _stateMachine.ChangeState(_player.FallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        _player.CanDash = true;
        _player.Rb.gravityScale = _originalGravity;
        _player.Rb.linearVelocity = Vector2.zero;
    }
}