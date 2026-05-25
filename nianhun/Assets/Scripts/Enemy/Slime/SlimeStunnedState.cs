using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStunnedState : EnemyState
{
    protected Slime enemy;
    public SlimeStunnedState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
    }

    public override void enter()
    {
        base.enter();
        enemy.fx.InvokeRepeating("redcolourblink", 0, .1f);

        statetimer = enemy.stuntime;


    }

    public override void exit()
    {
        base.exit();

        enemy.fx.Invoke("cancelcolorchange", 0);
        enemy.Stat.MakeInvincible(false);
    }

    public override void update()
    {
        base.update();

        if (rb.velocity.y < .1f && enemy.isgrounddetected())
        {
            enemy.anim.SetTrigger("Stun");
            enemy.Stat.MakeInvincible(true);
        }

        if (statetimer < 0)
        {

            statemachine.changestate(enemy.idlestate);
        }
    }

}
