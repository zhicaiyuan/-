using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonBattleState : EnemyState
{
    private Transform player;
    private Skeleton enemy;
    private int movedir;

    private float flipCooldown = 0f;
    [SerializeField] private float flipDelay = .2f;
    [SerializeField] private float stopTurnDistance = .8f;
    
    
    public SkeletonBattleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();


        player = playermanger.instance.player.transform; //»∑»œpalyerŒª÷√
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

        if (enemy.ispalyerdetected()) //ºÏ≤‚ÕÊº“
        {
            statetimer = enemy.battletime;
            if (enemy.ispalyerdetected().distance < enemy.attackcheckdistance && canattack()) //ø…π•ª˜∑∂Œß,¿‰»¥◊„πª
            {
                enemy.isattack = true;
                statemachine.changestate(enemy.attackstate);
                AudioManager.instance.PlaySFX(5,null);
            }
           

        }
        else
        {
            if (statetimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 20)
                statemachine.changestate(enemy.idlestate);
        }
        if (flipCooldown > 0)
            flipCooldown -= Time.deltaTime;
        
        if(Vector2.Distance(player.position,enemy.transform.position) > stopTurnDistance)
        {
            if (player.position.x > enemy.transform.position.x)//ÕÊº“‘⁄”“≤‡
            {
                movedir = 1;
            }
            else if (player.position.x < enemy.transform.position.x)//ÕÊº“‘⁄◊Û≤‡
            {
                movedir = -1;
            }
        }
        else
        {
            movedir = 0;
        }

        if (rb.velocity.y==0)
            enemy.setvelocity(movedir * enemy.movespeed, rb.velocity.y);//“∆∂Ø
        //∑≠◊™
        if (rb.velocity.x > 0 && !enemy.faceright)
        {
            enemy.flip();
            flipCooldown = flipDelay;
        }
        else if (rb.velocity.x < 0 && enemy.faceright)
        {
            enemy.flip();
            flipCooldown = flipDelay;
        }
    }

    private bool canattack()//ºÏ≤‚π•ª˜¿‰»¥
    {
        if(Time.time >= enemy.lasttimeattack + enemy.attackcooldown)
        {
            enemy.attackcooldown = Random.Range(enemy.minattackcooldown, enemy.maxattackcooldown);
            enemy.lasttimeattack = Time.time;
            return true;

        }
        return false;
    }
}
