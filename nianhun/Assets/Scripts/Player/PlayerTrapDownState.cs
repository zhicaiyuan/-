using UnityEngine;

public class PlayerTrapDownState : PlayerState
{
    public PlayerTrapDownState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.zerovelocity();
    }

    public override void Update()
    {
        base.Update();
        player.zerovelocity();
    }

    public override void Exit()
    {
        player.anim.speed = 1f;
        base.Exit();
    }
}
