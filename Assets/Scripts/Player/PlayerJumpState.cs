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

        // _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, _player.Data.jumpForce);
        _player.Rb.linearVelocity = new Vector2(_player.Rb.linearVelocity.x, 0);
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
        // di chuyển ngang khi giữ nút di chuyển
        float targetSpeed = _player.MoveX * _player.Data.maxMoveSpeed;
        float accelerationRate = Mathf.Abs(_player.MoveX) > 0.1f ? _player.Data.acceleration : 0f;
        float newVelocityX = Mathf.MoveTowards(_player.Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        _player.Rb.linearVelocity = new Vector2(newVelocityX, _player.Rb.linearVelocity.y);

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
