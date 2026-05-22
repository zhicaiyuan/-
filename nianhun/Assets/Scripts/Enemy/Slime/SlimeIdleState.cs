using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeIdleState : SlimeGroundState
{
    public SlimeIdleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname, slime)
    {
    }


// Start is called before the first frame update
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
