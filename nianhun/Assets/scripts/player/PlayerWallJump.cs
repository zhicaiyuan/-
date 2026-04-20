using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJump : PlayerState
{
    public PlayerWallJump(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();

        statetimer = .4f;
        player.setvelocity(9 * -player.facedir, player.jumpforce);
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();

        if (statetimer < 0)
            statemachine.changestate(player.airstate);

        if (player.isgrounddetected())
        {
            statemachine.changestate(player.idlestate);
        }
    }

    
}
