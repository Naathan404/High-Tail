using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float _dashTimer;
    private float _originalGravity;

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        if(!_player.CanDash) return;
        Debug.Log("<color=yellow> ==> Dashing ==> </Color>");
        _originalGravity = _player.Rb.gravityScale;
        _player.Rb.gravityScale = 1f;
        _player.Visual.ApplySquashStretch(new Vector3(1.3f, 0.8f, 1f), 0.05f);

        // time freeze
        if(!_player.IsOnGround())
        GameManager.Instance.DoTimeFreeze(0.05f, 0.05f);

        _dashTimer = _player.Data.dashDuration;
        Vector2 direction = _player.IsFacingRight() ? Vector2.right : Vector2.left;
        _player.Rb.linearVelocity = direction * _player.Data.dashForce; 

        _player.CanDash = false;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        if(_player.WallJumpUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.JumpPressed)
        {
            _stateMachine.ChangeState(_player.WallJumpState);
        }
        if(_player.WallSlideUnlocked && _player.IsTouchingWall() && !_player.IsOnGround() && _player.SlideGlideHeld)
        {
            _stateMachine.ChangeState(_player.WallSlideState);
        }                       
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if(_player.IsTouchingWall())
        {
            PerformWallBounce();
            return;
        }

        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0)
        {
            if (_player.IsOnGround())
                _stateMachine.ChangeState(_player.IdleState);
            else
                _stateMachine.ChangeState(_player.FallState);
        }

    }

    public override void Exit()
    {
        base.Exit();
        
        _player.CanDash = true;
        _player.Rb.gravityScale = _originalGravity;
        _player.Rb.linearVelocity = Vector2.zero;
    }

    private void PerformWallBounce()
    {
        float bounceDir = _player.IsFacingRight() ? -1f : 1f;
        Debug.Log("Chạm tường! Facing: " + _player.IsFacingRight() + " | Bounce Dir: " + bounceDir);
        _player.Rb.linearVelocity = Vector2.zero;
        Vector2 bounceVector = new Vector2(
            _player.Data.bounceHorizontalForce * bounceDir, 
            _player.Data.bounceVerticalForce
        );
        _player.Rb.AddForce(bounceVector, ForceMode2D.Impulse);

        _player.Visual.ApplySquashStretch(new Vector3(1.3f, 0.7f, 1f)); 
    }    
}