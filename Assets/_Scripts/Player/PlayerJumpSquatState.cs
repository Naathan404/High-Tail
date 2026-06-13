using UnityEngine;

public class PlayerJumpSquatState : PlayerState
{
    private float _waitTimer = 0.04f;
    private float _timer = 0f;
    public PlayerJumpSquatState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {}

    public override void Enter()
    {
        base.Enter();
        _player.Visual.Anim.Play("pJumpSquat");
        _player.Rb.linearVelocity *= 0.5f;
        _timer = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _timer += Time.deltaTime;
        if(_timer > _waitTimer)
        {
            _timer = 0f;
            _stateMachine.ChangeState(_player.JumpState);
            return;
        }
    }
}
