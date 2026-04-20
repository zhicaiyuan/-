using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonStunnedState : EnemyState
{
    private Skeleton enemy;
    public SkeletonStunnedState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname,Skeleton enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        enemy.fx.InvokeRepeating("redcolourblink", 0, .1f);

        statetimer = enemy.stuntime;

        
    }

    public override void exit()
    {
        base.exit();

        enemy.fx.Invoke("cancelcolorchange", 0);
    }

    public override void update()
    {
        base.update();

        if(statetimer < 0)
            statemachine.changestate(enemy.idlestate);
    }

    public static implicit operator SkeletonStunnedState(SkeleonGroundState v)
    {
        throw new NotImplementedException();
    }
}
