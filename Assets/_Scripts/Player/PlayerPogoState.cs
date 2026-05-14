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
    private float _timeToMakeLandingEffect = 0.75f;
    private float _timer = 0f;
    public PlayerPogoState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {}

    public override void Enter()
    {
        base.Enter();
        AudioManager.Instance.PlaySFX(SoundName.Player_Stomp);
        _player.Visual.Anim.Play("pPogo");
        
        _player.Rb.linearVelocity = Vector2.zero; 
        _player.Rb.gravityScale = 0f; 
        
        _player.Rb.linearVelocity = new Vector2(0f, -_player.Data.maxSpeedY * 1.4f);

        _player.Visual.PogoDustParticle.Play();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
                _timer += Time.deltaTime;

        if (_player.IsPogoHit())
        {
            _player.ExecutePogoBounce();
            _stateMachine.ChangeState(_player.BounceState);
            CameraShakeManager.Instance.ShakeForLanding(); 
            _player.Visual.LandingDustParticle.gameObject.SetActive(true);
            _player.Visual.LandingDustParticle.Play();   
        }
        else if (_player.IsOnGround())
        {
            if (_timer >= _timeToMakeLandingEffect)
            {
                _player.TriggerHardLanding();
                _timer = 0f;
            }
            else
            {
                CameraShakeManager.Instance.ShakeCustom(0.3f); 
                _player.Visual.LandingDustParticle.gameObject.SetActive(true);
                _player.Visual.LandingDustParticle.Play();  
                _timer = 0f;
            }
            

            if (Mathf.Abs(_player.MoveX) > 0.01f)
            {
                _stateMachine.ChangeState(_player.RunState);
            }
            else
            {
                _stateMachine.ChangeState(_player.IdleState);
                _player.Rb.linearVelocity = new Vector2(0f, _player.Rb.linearVelocity.y);
            }
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