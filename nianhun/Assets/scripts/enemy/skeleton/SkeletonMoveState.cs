using UnityEngine;

public class SkeletonMoveState : SkeleonGroundState
{
    private float flipCooldown;
    private const float flipDelay = 0.2f;

    public SkeletonMoveState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy)
        : base(_enemybase, _statemachine, _animboolname, enemy)
    {
    }

    public override void enter()
    {
        base.enter();
        flipCooldown = 0f;
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
