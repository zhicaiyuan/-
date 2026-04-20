using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState :PlayerState
{
    public PlayerGroundState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
        if(Input.GetKeyDown(KeyCode.U))
        {
            statemachine.changestate(player.counterattackstate);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            statemachine.changestate(player.primaryattack);
        }

        if(Input.GetKeyDown(KeyCode.K) && player.isgrounddetected() && !player.iswalldetected())
        {
            statemachine.changestate(player.jumpstate);
        }
       if(!player.isgrounddetected())
        {
            statemachine.changestate(player.airstate);
        }
    }

   
}
