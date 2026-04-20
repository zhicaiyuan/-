using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();
        if (player.iswalldetected())
        {
            statemachine.changestate(player.wallslide);
        }
        rb.velocity = new Vector2(rb.velocity.x, player.jumpforce);
    }

    public override void exit()
    {
        base.exit();
 
    }

    public override void update()
    {
        base.update();

        if(rb.velocity.y < 0)
        {
            statemachine.changestate(player.airstate);
        }
        if (player.iswalldetected())
        {
            statemachine.changestate(player.wallslide);
        }
        if (xinput != 0)
        {
            player.setvelocity(player.movespeed * xinput, rb.velocity.y);
        }
    }
}
