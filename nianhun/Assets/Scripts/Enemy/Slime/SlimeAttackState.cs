using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackState : EnemyState
{
    protected Slime enemy;
    public SlimeAttackState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
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
