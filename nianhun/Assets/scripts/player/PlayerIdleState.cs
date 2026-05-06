using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.zerovelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
   
        
        if(xinput != 0 && !player.isbusy)
        {
            statemachine.changestate(player.movestate);
        }
    }
}
