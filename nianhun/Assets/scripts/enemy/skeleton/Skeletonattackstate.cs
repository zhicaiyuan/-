using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeletonattackstate : EnemyState
{
    private Skeleton enemy;

    public Skeletonattackstate(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname,Skeleton enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
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

        if(triggercalled)
        {
            enemy.isattack = false;
            statemachine.changestate(enemy.battlestate);
        }
    }
}
