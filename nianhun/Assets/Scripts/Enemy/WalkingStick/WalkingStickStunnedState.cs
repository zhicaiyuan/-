using UnityEngine;

public class WalkingStickStunnedState : EnemyState
{
    private WalkingStick enemy;

    public WalkingStickStunnedState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, WalkingStick walkingStick)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = walkingStick;
    }

    public override void enter()
    {
        base.enter();
        enemy.fx.InvokeRepeating("redcolourblink", 0, .1f);
        statetimer = enemy.stuntime;
    }

    public override void exit()
    {
        base.exit();
        enemy.fx.Invoke("cancelcolorchange", 0);
    }

    public override void update()
    {
        base.update();

        if (statetimer < 0)
        {
            if (enemy.IsPhase2)
                statemachine.changestate(enemy.walkstate);
            else
                statemachine.changestate(enemy.idlestate);
        }
    }
}
