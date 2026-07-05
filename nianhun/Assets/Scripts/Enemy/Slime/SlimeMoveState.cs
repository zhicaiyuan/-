using UnityEngine;

public class SlimeMoveState : SlimeGroundState
{
    private float flipCooldown;

    public SlimeMoveState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime)
        : base(_enemybase, _statemachine, _animboolname, slime)
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
