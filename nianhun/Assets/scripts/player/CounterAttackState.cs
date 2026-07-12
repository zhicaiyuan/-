using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterAttackState : PlayerState
{
    
    public CounterAttackState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        AudioManager.instance.PlaySFX(18, null);
        player.Stat.MakeInvincible(true);
        player.isUnstoppable = true;
        player.couterattacktimer = Time.time;
        statetimer = player.counterattackduration;

        player.anim.SetBool("successfulattack",false);
    }

    public override void Exit()
    {
        base.Exit();
        player.Stat.MakeInvincible(false);
        player.isUnstoppable = false;
    }

    public override void Update()
    {
        base.Update();
        player.zerovelocity();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackcheck.position, player.attackcheckradius);

        foreach (var hit in colliders)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null)
                continue;

            if (enemy.canbestun())
            {
                statetimer = 10;
                player.anim.SetBool("successfulattack", true);
                AudioManager.instance.PlaySFX(16, null);
                AudioManager.instance.PlaySFX(17, null);
                player.fx.ScreenShake();
                Enemystat target = hit.GetComponentInParent<Enemystat>();
                player.Stat.Dotimesdamage(target, 2f);
                HitStopManager.instance.DoHitStop(.1f, .3f);
            }
        }

        if(statetimer < 0 || triggercalled)
        {
           
            statemachine.changestate(player.idlestate);
        }
    }
}
