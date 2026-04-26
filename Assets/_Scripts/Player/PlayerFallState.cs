using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class PlayerFallState : PlayerState
{
    private float _timeToMakeLandingEffect = 0.8f;
    private float _timer = 0f;
    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào fall state");
        _timer = 0f;
        _player.Visual.Anim.Play("playerFall");
        _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // timer timer timer
        _timer += Time.deltaTime;

        _player.CheckFlip(_player.MoveX);
        // Kiểm tra tiếp đất
        if (_player.IsOnGround())
        {
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

        // // Kiểm tra bám tường
        // if (_player.IsTouchingWall() && _player.canWallSlide && _player.InputX != 0) {
        //     _stateMachine.ChangeState(_player.WallSlideState);
        //     return;
        // }

        // Kiểm tra Dash trên không
        // if (_player.DashPressed && _player.CanDash && _player.CurrentEnergy >= 20) {
        //     _stateMachine.ChangeState(_player.DashState);
        //     return;
        // }

        // set jump buffer
        if (_player.JumpPressed)
        {
            _player.SetJumpBufferTimer();
        }
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if (_player.MoveY < -0.5f && !_player.IsOnGround() && _player.Data.PogoUnlocked)
        {
            _stateMachine.ChangeState(_player.PogoState);
            return;
        }

        if (_player.IsSlipWall) return;
        if (_player.DashPressed && _player.CanDash && _player.Data.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
            return;
        }
        if (_player.Data.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && !_player.IsSlipWall && _player.JumpPressed)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
            return;
        }
        if (_player.Data.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.GrabHeld && !_player.IsSlipWall)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
            return;
        }
        if (_player.Data.AirGlideUnlocked && !_player.IsTouchingWall() && !_player.IsOnGround() && _player.GlideHeld)
        {
            _stateMachine.ChangeState(_player.AirGlideState);
            return;
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        _player.HandleAirMovement();

        // Tăng trọng lực khi rơi
        if (_player.Rb.linearVelocity.y < 0)
        {
            _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
        }
        // // Air control
        // _player.HandleAirMovement();
    }

    public override void Exit()
    {
        base.Exit();
        if(_timer >= _timeToMakeLandingEffect)
        {
            CameraShakeManager.Instance.ShakeForLanding();
            _player.Visual.LandingDustParticle.gameObject.SetActive(true);
            _player.Visual.LandingDustParticle.Play();
            _timer = 0f;
        }
        else
        {
            _player.Visual.FallDustParticle.gameObject.SetActive(true);
            _player.Visual.FallDustParticle.Play();
            _timer = 0f;
        }
        _player.Visual.ApplySquashStretch(new Vector3(1.3f, 0.8f, 1f));
    }
}
