using UnityEngine;
using System.Collections;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {}

    public override void Enter()
    {
        base.Enter();
        
        _player.Rb.linearVelocity = Vector2.zero;
        _player.Rb.simulated = false;

        CameraShaker.Instance.OneTimeShake(Vector2.right, 0.2f);
        
        _player.Visual.Anim.Play("playerDie");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.simulated = true;
    }
}