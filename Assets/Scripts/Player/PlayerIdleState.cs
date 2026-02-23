using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào idle state");
        _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }

        if (Mathf.Abs(_player.MoveX) > 0.01f)
        {
            _stateMachine.ChangeState(_player.RunState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _player.CheckIfShoundFlip(_player.MoveX);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleHorizontalMovement();
    }
}
