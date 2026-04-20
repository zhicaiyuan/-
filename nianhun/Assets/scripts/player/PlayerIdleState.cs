using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();

        player.zerovelocity();
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
   
        
        if(xinput != 0 && !player.isbusy)
        {
            statemachine.changestate(player.movestate);
        }
    }
}
