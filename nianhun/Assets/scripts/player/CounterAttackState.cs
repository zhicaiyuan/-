using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterAttackState : PlayerState
{
    
    public CounterAttackState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void enter()
    {
        base.enter();
        AudioManager.instance.PlaySFX(18, null);

        statetimer = player.counterattackduration;

        player.anim.SetBool("successfulattack",false);
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
        player.zerovelocity();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackcheck.position, player.attackcheckradius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                if(hit.GetComponent<Enemy>().canbestun())
                {
                    statetimer = 10;
                    player.anim.SetBool("successfulattack", true);
                    AudioManager.instance.PlaySFX(16, null);
                    AudioManager.instance.PlaySFX(17, null);
                    player.fx.ScreenShake();
                    enemystat target = hit.GetComponent<enemystat>();
                    player.Stat.Dodamage(target);
                    HitStopManager.instance.DoHitStop(.1f, .3f);
                }
            }
        }

        if(statetimer < 0 || triggercalled)
        {
           
            statemachine.changestate(player.idlestate);
        }
    }
}
