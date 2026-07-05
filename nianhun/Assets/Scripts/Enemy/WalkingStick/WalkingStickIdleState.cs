using UnityEngine;

public class WalkingStickIdleState : EnemyState
{
    private WalkingStick enemy;

    public WalkingStickIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, WalkingStick walkingStick)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = walkingStick;
    }

    public override void enter()
    {
        base.enter();
        enemy.zerovelocity();
        enemy.SetSuperArmor(true);
        enemy.SyncGroundAnimator(false);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (enemy.IsPhase2)
        {
            statemachine.changestate(enemy.walkstate);
            return;
        }

        if (!enemy.IsPlayerDetected())
            return;

        enemy.FacePlayer();

        if (!enemy.HasUsedAttack1)
        {
            enemy.CurrentAttack = WalkingStickAttackType.Attack1;
            enemy.ConsumeAttackCooldown();
            statemachine.changestate(enemy.attackstate);
            return;
        }

        if (enemy.IsPlayerDetected() && enemy.IsAttackReady())
        {
            enemy.CurrentAttack = WalkingStickAttackType.Attack2;
            enemy.ConsumeAttackCooldown();
            statemachine.changestate(enemy.attackstate);
        }
    }
}
