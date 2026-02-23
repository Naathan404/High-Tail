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
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        _player.CheckIfShoundFlip(_player.MoveX);

        if(Mathf.Abs(_player.MoveX) < 0.01f && Mathf.Abs(_player.Rb.linearVelocity.x) < 0.1f)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleHorizontalMovement();
    }    
}
