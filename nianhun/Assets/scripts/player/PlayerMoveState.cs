using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundState
{
    public PlayerMoveState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();
        AudioManager.instance.PlaySFX(3,null);
    }

    public override void exit()
    {
        base.exit();
        AudioManager.instance.StopSFX(3);
    }

    public override void update()
    {
        base.update();



        if(xinput == 0 )
        {
            statemachine.changestate(player.idlestate);
        }
        player.setvelocity(xinput * player.movespeed,rb.velocity.y);
    }
}
