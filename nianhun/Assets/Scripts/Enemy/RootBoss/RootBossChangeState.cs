using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootBossChangeState : EnemyState
{
    private RootBoss enemy;

    public RootBossChangeState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname) : base(_enemybase, _statemachine, _animboolname)
    {
    }

    public RootBossChangeState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss rootboss) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = rootboss;
    }
}
