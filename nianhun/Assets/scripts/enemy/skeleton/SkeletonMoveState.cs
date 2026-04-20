using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonMoveState : SkeleonGroundState
{
    public SkeletonMoveState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy) : base(_enemybase, _statemachine, _animboolname, enemy)
    {
    }

    public override void enter()
    {
        base.enter();
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();

        if (!enemy.isgrounddetected())
        {
            enemy.flip();
            statemachine.changestate(enemy.movestate);
        }
        enemy.setvelocity(enemy.movespeed * enemy.facedir, enemy.rb.velocity.y);

        if(enemy.iswalldetected())
        {
            enemy.flip();
            statemachine.changestate(enemy.movestate);
        }

        
    }
}
