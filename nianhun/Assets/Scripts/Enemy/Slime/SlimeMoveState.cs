using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMoveState : SlimeGroundState
{
    private float flipCooldown;
    private const float flipDelay = 0.2f;

    public SlimeMoveState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime) : base(_enemybase, _statemachine, _animboolname, slime)
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

        if (flipCooldown > 0f)
            flipCooldown -= Time.deltaTime;

        if (!enemy.isgrounddetected() && flipCooldown <= 0f)
        {
            enemy.Flip();
            flipCooldown = flipDelay;
        }

        enemy.setvelocity(enemy.movespeed * enemy.facedir, enemy.rb.velocity.y);

        if (enemy.iswalldetected() && flipCooldown <= 0f)
        {
            enemy.Flip();
            flipCooldown = flipDelay;
        }
    }
}
