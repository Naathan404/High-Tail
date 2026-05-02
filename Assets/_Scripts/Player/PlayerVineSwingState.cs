using UnityEngine;

public class PlayerVineSwingState : PlayerState
{
    private HingeJoint2D _grabJoint; // ĐỔI SANG HINGE JOINT!

    public PlayerVineSwingState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.CanDash = true;
        _player.Visual.Anim.Play("playerIdle");
        _player.Rb.gravityScale = _player.BaseGravity;

        _grabJoint = _player.gameObject.AddComponent<HingeJoint2D>();
        _grabJoint.connectedBody = _player.CurrentVineRb;

        // sóc bám vào dây
        _grabJoint.autoConfigureConnectedAnchor = false;
        
        // tâm bám của sợi dây nằm ở chính giữa đốt dây
        _grabJoint.connectedAnchor = Vector2.zero; 
        
        // Tâm bám của sóc nằm ở trên đỉnh đầu của nó
        _grabJoint.anchor = new Vector2(0f, 0.5f); 
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.CheckFlip(_player.MoveX);

        // truyền lực cho player
        if (Mathf.Abs(_player.MoveX) > 0.1f)
        {

            Vector2 forceDir = new Vector2(_player.MoveX, -0.2f).normalized; 
            
            _player.Rb.AddForce(forceDir * _player.Data.swingForce);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(!_player.GrabHeld)
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }
        if (_player.JumpPressed)
        {
            Vector2 currentMomentum = _player.Rb.linearVelocity;

            Object.Destroy(_grabJoint);
            _player.StartVineCooldown(); 

            // cộng đà của sóc và đà văng lên
            _player.Rb.linearVelocity = new Vector2(currentMomentum.x, currentMomentum.y + _player.Data.jumpForce);
            
            _stateMachine.ChangeState(_player.JumpState);
        }

        if(_player.Data.DashUnlocked && _player.CanDash && _player.DashPressed)
        {
            _player.StartVineCooldown();
            _stateMachine.ChangeState(_player.DashState);
        }

    }

    public override void Exit()
    {
        base.Exit();
        if (_grabJoint != null) Object.Destroy(_grabJoint);

        _player.CurrentVineRb = null;
        _player.CurrentVineTransform = null;
    }
}