using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBattleState : EnemyState
{
    protected Slime enemy;
    private Transform player;
    private int movedir;

    private float flipCooldown = 0f;
    [SerializeField] private float flipDelay = .1f;
    [SerializeField] private float stopTurnDistance = .8f;
    private const float turnDeadzone = 0.15f;
    public SlimeBattleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
    }
    public override void enter()
    {
        base.enter();


        player = playermanger.instance.player.transform; //确认palyer位置
        if (player.GetComponent<PlayerStat>().isdead)
            statemachine.changestate(enemy.movestate);
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();

        if (enemy.ispalyerdetected()) //检测玩家
        {
            statetimer = enemy.battletime;
            if (enemy.ispalyerdetected().distance < enemy.attackcheckdistance && canattack()) //可攻击范围,冷却足够
            {
                enemy.isattack = true;
                statemachine.changestate(enemy.attackstate);
                AudioManager.instance.PlaySFX(5, null);
            }


        }
        else
        {
            if (statetimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 20)
                statemachine.changestate(enemy.idlestate);
        }
        if (flipCooldown > 0)
            flipCooldown -= Time.deltaTime;

        if (Vector2.Distance(player.position, enemy.transform.position) > stopTurnDistance)
        {
            float dx = player.position.x - enemy.transform.position.x;
            if (Mathf.Abs(dx) < turnDeadzone)
                movedir = 0;
            else if (dx > 0)
                movedir = 1;
            else
                movedir = -1;
        }
        else
        {
            movedir = 0;
        }

        if (rb.velocity.y == 0)
            enemy.setvelocity(movedir * enemy.movespeed, rb.velocity.y);//移动

        if (flipCooldown <= 0f && movedir != 0)
        {
            if (movedir > 0 && !enemy.faceright)
            {
                enemy.flip();
                flipCooldown = flipDelay;
            }
            else if (movedir < 0 && enemy.faceright)
            {
                enemy.flip();
                flipCooldown = flipDelay;
            }
        }
    }

    private bool canattack()//检测攻击冷却
    {
        if (Time.time >= enemy.lasttimeattack + enemy.attackcooldown)
        {
            enemy.attackcooldown = Random.Range(enemy.minattackcooldown, enemy.maxattackcooldown);
            enemy.lasttimeattack = Time.time;
            return true;

        }
        return false;
    }

}
