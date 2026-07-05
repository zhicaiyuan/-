using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeIdleState : SlimeGroundState
{
    public SlimeIdleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname, slime)
    {
    }


public override void enter()
    {
        base.enter();
        EnemyPatrolMoveHelper.EnterPatrolIdle(enemy, ref statetimer);
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
