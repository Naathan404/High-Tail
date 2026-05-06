// using System.Numerics;

// public class PlayerPogoState : PlayerState
// {
//     public PlayerPogoState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {}

//     public override void Enter()
//     {
//         base.Enter();
//         _player.Visual.Anim.Play("playerRoll");
//     }

//     public override void LogicUpdate()
//     {
//         base.LogicUpdate();
//         if(_player.MoveY > -0.1f && !_player.IsOnGround())
//         {
//             _stateMachine.ChangeState(_player.FallState);
//             return;
//         }


//         if (_player.IsPogoHit())
//         {
//             _player.ExecutePogoBounce();
//             _stateMachine.ChangeState(_player.JumpState);
//         }
//         else if (_player.IsOnGround())
//         {
//             _stateMachine.ChangeState(_player.IdleState);
//         }
//     }

//     public override void PhysicsUpdate()
//     {
//         base.PhysicsUpdate();
//         _player.HandleAirMovement();
//     }
// }


using UnityEngine;

public class PlayerPogoState : PlayerState
{
    public PlayerPogoState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {}

    public override void Enter()
    {
        base.Enter();
        _player.Visual.Anim.Play("playerRoll");
        
        _player.Rb.linearVelocity = Vector2.zero; 
        _player.Rb.gravityScale = 0f; 
        
        _player.Rb.linearVelocity = new Vector2(0f, -_player.Data.maxSpeedY * 1.5f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        

        if (_player.IsPogoHit())
        {
            _player.ExecutePogoBounce();
            _stateMachine.ChangeState(_player.JumpState);
        }
        else if (_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.IdleState);
            CameraShakeManager.Instance.ShakeForLanding(); // Rung màn hình vì dậm hụt
            _player.Visual.LandingDustParticle.Play();     // Bốc bụi mù mịt
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        _player.Rb.gravityScale = _player.BaseGravity;
    }
}