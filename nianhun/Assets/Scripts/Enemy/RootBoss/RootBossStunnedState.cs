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
        enemy.anim.SetBool("Stun", true);
        enemy.zerovelocity();
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
        if (triggercalled)
        {
            statemachine.changestate(enemy.battlestate);
        }
    }
}
