using UnityEngine;

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Vào fall state");
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        _player.CheckFlip(_player.MoveX);
        // Kiểm tra tiếp đất
        if (_player.IsOnGround()) {
            if (Mathf.Abs(_player.MoveX) > 0.01f)
            {
                _stateMachine.ChangeState(_player.RunState);
            }
            else
            {
                _stateMachine.ChangeState(_player.IdleState);
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
        if (_player.JumpPressed) {
            _player.SetJumpBufferTimer();
        }
    }


    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        // di chuyển ngang khi giữ nút di chuyển
        float targetSpeed = _player.MoveX * _player.Data.maxMoveSpeed;
        float accelerationRate = Mathf.Abs(_player.MoveX) > 0.1f ? _player.Data.acceleration : 0f;
        float newVelocityX = Mathf.MoveTowards(_player.Rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        _player.Rb.linearVelocity = new Vector2(newVelocityX, _player.Rb.linearVelocity.y);
        
        // Tăng trọng lực khi rơi
        if (_player.Rb.linearVelocity.y < 0) {
            _player.Rb.gravityScale = _player.Data.gravityScale * _player.Data.fallMultiplier;
        }
        
        // // Air control
        // _player.HandleAirMovement();
    }
}
