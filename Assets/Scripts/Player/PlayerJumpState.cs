using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }
    public override void Enter()
    {
        base.Enter();
        // 
        Debug.Log("Vào jump state");
        _player.UseJumpBuffer();
        _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, _player.Data.jumpForce);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _player.CheckIfShoundFlip(_player.MoveX);

        if(_player.Rb.linearVelocity.y < 0f)
        {
            _stateMachine.ChangeState(_player.FallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
