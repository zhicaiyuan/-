using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonIdleState : SkeleonGroundState
{
    public SkeletonIdleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy) : base(_enemybase, _statemachine, _animboolname, enemy)
    {
    }

    public override void enter()
    {
        base.enter();
        
        statetimer = enemy.idletime;
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
        if (statetimer < 0f)
        {
            statemachine.changestate(enemy.movestate);
        }

    }
}
