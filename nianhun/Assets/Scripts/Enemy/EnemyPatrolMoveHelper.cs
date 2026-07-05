using System;
using UnityEngine;

public static class EnemyPatrolMoveHelper
{
    private const float FlipDelay = 0.2f;

    public static void EnterPatrolMove(Enemy enemy, ref float flipCooldown)
    {
        flipCooldown = 0f;
    }

    public static void EnterPatrolIdle(Enemy enemy, ref float idleTimer)
    {
        enemy.ApplyPatrolTurnAroundOnIdleEnter();
        idleTimer = enemy.idletime;
    }

    public static void UpdatePatrolMove(
        Enemy enemy,
        ref float flipCooldown,
        ref float walkTimer,
        Action enterIdle)
    {
        if (flipCooldown > 0f)
            flipCooldown -= Time.deltaTime;

        if (!enemy.isgrounddetected() && flipCooldown <= 0f)
        {
            enemy.Flip();
            flipCooldown = FlipDelay;
            walkTimer = enemy.WalkTime;
        }
        else if (enemy.iswalldetected() && flipCooldown <= 0f)
        {
            enemy.Flip();
            flipCooldown = FlipDelay;
            walkTimer = enemy.WalkTime;
        }

        enemy.setvelocity(enemy.movespeed * enemy.facedir, enemy.rb.velocity.y);

        if (walkTimer > 0f)
            return;

        enemy.zerovelocity();
        enemy.QueuePatrolTurnAround();
        enterIdle();
    }
}
