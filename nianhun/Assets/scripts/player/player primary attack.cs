using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttack : PlayerState
{
    private int combocounter;
    private float lasttimeattack;
    private float combowindow = 2;
    private EntityFx fx;

    public PlayerPrimaryAttack(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
        fx = _player.GetComponent<EntityFx>();
    }

    public override void Enter()
    {
        base.Enter();

        AudioManager.instance.PlaySFX(0,null);

        xinput = 0;

        if (combocounter > 2 || Time.time >=  lasttimeattack + combowindow)
        {
            combocounter = 0;
        }

        player.anim.SetInteger("combocounter", combocounter);
        fx.CreateAttackFx(player.transform, combocounter);


        float attackdir = player.facedir;

        if (xinput != 0)
        {
            attackdir = xinput;
        }
        
         player.setvelocity(player.attackmovement[combocounter].x * attackdir, player.attackmovement[combocounter].y);

        statetimer = .1f;
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("busyfor", .15f);

        combocounter++;
        lasttimeattack = Time.time;
    }

    public override void Update()
    {
        base.Update();

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
