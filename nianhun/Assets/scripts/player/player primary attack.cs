using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttack : PlayerState
{
    private int combocounter;
    private float lasttimeattack;
    private float combowindow = 2;

    public PlayerPrimaryAttack(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();

        AudioManager.instance.PlaySFX(0,null);

        xinput = 0;

        if (combocounter > 2 || Time.time >=  lasttimeattack + combowindow)
        {
            combocounter = 0;
        }

        player.anim.SetInteger("combocounter", combocounter);


        float attackdir = player.facedir;

        if (xinput != 0)
        {
            attackdir = xinput;
        }
        
         player.setvelocity(player.attackmovement[combocounter].x * attackdir, player.attackmovement[combocounter].y);

        statetimer = .1f;
    }

    public override void exit()
    {
        base.exit();

        player.StartCoroutine("busyfor", .15f);

        combocounter++;
        lasttimeattack = Time.time;
    }

    public override void update()
    {
        base.update();

        if(statetimer < 0)
        {
            player.zerovelocity();
        }

        if(triggercalled)
        {
            statemachine.changestate(player.idlestate);
        }
    }
}
