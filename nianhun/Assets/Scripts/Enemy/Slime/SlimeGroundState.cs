using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGroundState : EnemyState
{
    protected Slime enemy;
    protected Transform player;
    public SlimeGroundState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname,Slime slime) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
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

        if (enemy.ispalyerdetected() || Vector2.Distance(enemy.transform.position, player.position) < 2)//切换攻击模式
            statemachine.changestate(enemy.battlestate);
    }
}
