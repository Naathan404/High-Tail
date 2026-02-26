using UnityEngine;

public class PlayerAirGlideState : PlayerState
{
    public PlayerAirGlideState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Đã vào air glide state");
    }
}
