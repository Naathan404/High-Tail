using UnityEngine;

public class PlayerAirGlideState : PlayerState
{
    private float _originalGravityScale;
    public PlayerAirGlideState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Đã vào air glide state");
        _originalGravityScale = _player.Rb.gravityScale;
        _player.Rb.gravityScale = _player.Data.airGlideGravityScale;
        _player.Rb.linearVelocity = Vector2.zero;

    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!_player.SlideGlideHeld)
        {
            _stateMachine.ChangeState(_player.FallState);
        }
        if(_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
        _player.CheckFlip(_player.MoveX);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float targetSpeed = _player.MoveX * _player.Data.maxGlideSpeed;
        float accelerationRate = (Mathf.Abs(_player.MoveX) > 0.1f) ? _player.Data.glideAccelaration : 0f;
        float newVelocityX = Mathf.MoveTowards(_player.Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        
        _player.Rb.linearVelocity = new Vector2(newVelocityX, _player.Rb.linearVelocity.y);        
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _originalGravityScale;
    }
}
