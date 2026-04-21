using UnityEngine;

public class PlayerRunState : PlayerState
{
    private float _maxSpeed;
    public PlayerRunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào run state");
        _player.CanDash = true;

        _player.Visual.Anim.Play("playerRun");
        _player.Visual.RunDustParticle.Play();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        _player.CheckFlip(_player.MoveX);

        if(Mathf.Abs(_player.MoveX) < 0.01f && Mathf.Abs(_player.Rb.linearVelocity.x) < 0.1f)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }

        if(_player.CanJump())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
        if(_player.Visual.RunDustParticle.isPlaying) return;

    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(_player.DashPressed && _player.CanDash && _player.Data.DashUnlocked)
        {
            _stateMachine.ChangeState(_player.DashState);
            return;
        }
        if(_player.Data.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.JumpPressed)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
            return;
        }
        if(_player.Data.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.GrabHeld)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
            return;
        }        
    }    

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleHorizontalMovement();
    }

    public override void Exit()
    {
        base.Exit();
        _player.Visual.RunDustParticle.Stop();
    }
}
