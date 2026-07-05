using UnityEngine;

public class RootBossMoveState : RootBossGroundState
{
    private float flipCooldown;

    public RootBossMoveState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy)
        : base(_enemybase, _statemachine, _animboolname, enemy)
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
