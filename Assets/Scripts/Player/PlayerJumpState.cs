using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào jump state");
        _player.UseJumpBuffer();

        float boost = Mathf.Abs(_player.MoveX) > 0.9f ? _player.Data.boostVelocity : 1f;
        // _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, _player.Data.jumpForce);
        _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x * boost, 0);
        _player.Rb.AddForce(Vector2.up * _player.Data.jumpForce, ForceMode2D.Impulse);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _player.CheckFlip(_player.MoveX);
        if(_player.Rb.linearVelocity.y < 0f)
        {
            _stateMachine.ChangeState(_player.FallState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        _player.HandleAirMovement();
        // dynamic jump
        if(!_player.JumpHeld && _player.Rb.linearVelocity.y > 0f)
        {
            _player.Rb.linearVelocity += Vector2.up * Physics2D.gravity.y * _player.Data.fallMultiplier * Time.fixedDeltaTime;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
