using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootBossAttack1State : EnemyState
{
    private RootBoss enemy;

    public RootBossAttack1State(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = _enemybase as RootBoss;
    }


    public override void enter()
    {
        base.enter();

    }

    public override void exit()
    {
        base.exit();
        enemy.lasttimeattack = Time.time;
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled)
        {
            enemy.isattack = false;
            statemachine.changestate(enemy.battlestate);
        }
    }
}
