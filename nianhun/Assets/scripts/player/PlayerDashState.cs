using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();

        AudioManager.instance.PlaySFX(11, null);

        statetimer = player.dashduration;
    }

    public override void exit()
    {
        base.exit();

        player.setvelocity(0, rb.velocity.y);
    }

    public override void update()
    {
        base.update();
        

        player.setvelocity(player.dashspeed * player.dashdir, 0);

        if (player.iswalldetected() && !player.isgrounddetected())
        {
            Debug.Log("true");
            statemachine.changestate(player.wallslide);
        }
        if (statetimer < 0)
        {
            statemachine.changestate(player.idlestate);
        }
    }
}