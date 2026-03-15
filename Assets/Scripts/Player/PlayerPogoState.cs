public class PlayerPogoState : PlayerState
{
    public PlayerPogoState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {}

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(_player.MoveY > -0.1f && !_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.FallState);
            return;
        }
         

        if (_player.IsPogoHit())
        {
            _player.ExecutePogoBounce();
            _stateMachine.ChangeState(_player.JumpState);
        }
        else if (_player.IsOnGround())
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        _player.HandleAirMovement();
    }
}