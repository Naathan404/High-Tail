using NUnit.Framework.Constraints;
using UnityEngine;

public class PlayerVineClimbState : PlayerState
{
    public PlayerVineClimbState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.Visual.Anim.Play("playerIdle");
        _player.CanDash = true;

        if (_player.CurrentVineRb != null)
        {
            _player.CurrentVineRb.AddForce(new Vector2(_player.Data.impulseForce * _player.Rb.linearVelocity.normalized.x, 0f), ForceMode2D.Impulse);
        }        
        _player.Rb.gravityScale = 0f;
        _player.Rb.linearVelocity = Vector2.zero;

        _player.transform.position = new Vector3(_player.CurrentVineTransform.position.x, _player.transform.position.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        _player.CheckFlip(_player.MoveX);
        if(!_player.GrabHeld)
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }
        if(_player.JumpPressed)
        {
            if (_player.CurrentVineRb != null)
            {
                float kickbackForce = _player.IsFacingRight() ? -_player.Data.kickbackForce : _player.Data.kickbackForce; 
                _player.CurrentVineRb.AddForce(new Vector2(kickbackForce, 0f), ForceMode2D.Impulse);
            }
            _player.StartVineCooldown();
            _stateMachine.ChangeState(_player.JumpState);
            return;
        }

        if(_player.Data.DashUnlocked && _player.CanDash && _player.DashPressed)
        {
            _player.StartVineCooldown();
            _stateMachine.ChangeState(_player.DashState);
            return;
        }

        _player.transform.position = new Vector3(_player.CurrentVineTransform.position.x, _player.transform.position.y);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.Rb.linearVelocity = new Vector2(0f, _player.MoveY * _player.Data.climbForce);
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _player.BaseGravity;
    }

    private void AttachToVine(Rigidbody2D segment)
    {
        
    }
}
