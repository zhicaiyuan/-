using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
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

        if (player.iswalldetected() )
        {
            Debug.Log("true");
            statemachine.changestate(player.wallslide);
        }
       
        if (player.isgrounddetected())
        {
           
            statemachine.changestate(player.idlestate);
        }


        if(xinput != 0 )
        {
            player.setvelocity(player.movespeed *  xinput, rb.velocity.y);
        }
    }
}
