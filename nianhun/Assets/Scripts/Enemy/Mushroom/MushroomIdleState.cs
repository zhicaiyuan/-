using UnityEngine;

public class MushroomIdleState : MushroomGroundState
{
    public MushroomIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName, mushroom)
    {
    }

    public override void enter()
    {
        base.enter();
        EnemyPatrolMoveHelper.EnterPatrolIdle(enemy, ref statetimer);
    }

    public override void update()
    {
        base.update();

        if (statetimer < 0f)
            statemachine.changestate(enemy.movestate);
    }
}
