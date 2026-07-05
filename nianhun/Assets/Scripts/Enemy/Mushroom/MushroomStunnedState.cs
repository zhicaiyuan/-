using UnityEngine;

public class MushroomStunnedState : EnemyState
{
    private Mushroom enemy;

    public MushroomStunnedState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = mushroom;
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
            statemachine.changestate(enemy.idlestate);
    }
}
