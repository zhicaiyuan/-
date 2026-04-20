using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeleonGroundState : EnemyState
{
    protected Skeleton enemy;
    protected Transform player;
    public SkeleonGroundState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        player = playermanger.instance.player.transform;//确认player位置
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();

        if (enemy.ispalyerdetected() || Vector2.Distance(enemy.transform.position,player.position) < 2)//切换攻击模式
            statemachine.changestate(enemy.battlestate);
    }
}
