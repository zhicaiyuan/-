using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootBossStunnedState : EnemyState
{
    private RootBoss enemy;

    public RootBossStunnedState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname,RootBoss enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        triggercalled = false;
        enemy.zerovelocity();
        statetimer = enemy.stuntime > 0f ? enemy.stuntime : 1.5f;
        enemy.anim.SetBool("Stun", true);
    }

    public override void exit()
    {
        base.exit();
        enemy.anim.SetBool("Stun", false);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled || statetimer < 0f)
            statemachine.changestate(enemy.battlestate);
    }
}
