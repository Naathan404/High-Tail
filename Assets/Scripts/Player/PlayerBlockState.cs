using UnityEngine;

public class PlayerBlockState : PlayerState
{
    public PlayerBlockState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

public override void Enter()
    {
        base.Enter();
        
        _player.Rb.linearVelocity = Vector2.zero;
        _player.Rb.gravityScale = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!_player.IsBlocked)
        {
            if(_player.IsOnGround()) _stateMachine.ChangeState(_player.IdleState);
            else _stateMachine.ChangeState(_player.FallState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.Rb.linearVelocity = Vector2.MoveTowards(_player.Rb.linearVelocity, new Vector2(0f, _player.Rb.linearVelocity.y), _player.Data.decceleration * Time.fixedDeltaTime);
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
    }
}
