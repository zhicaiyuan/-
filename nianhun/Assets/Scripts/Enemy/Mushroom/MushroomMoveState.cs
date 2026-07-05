using UnityEngine;

public class MushroomMoveState : MushroomGroundState
{
    private float flipCooldown;

    public MushroomMoveState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName, mushroom)
    {
    }

    public override void enter()
    {
        base.enter();
        EnemyPatrolMoveHelper.EnterPatrolMove(enemy, ref flipCooldown);
        statetimer = enemy.WalkTime;
    }

    public override void update()
    {
        base.update();

        EnemyPatrolMoveHelper.UpdatePatrolMove(
            enemy,
            ref flipCooldown,
            ref statetimer,
            () => statemachine.changestate(enemy.idlestate));
    }
}
