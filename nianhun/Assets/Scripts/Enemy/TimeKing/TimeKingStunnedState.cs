using UnityEngine;

public class TimeKingStunnedState : EnemyState
{
    private TimeKing enemy;

    public TimeKingStunnedState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.zerovelocity();
        statetimer = enemy.stuntime > 0f ? enemy.stuntime : 1.5f;
        enemy.fx.InvokeRepeating("redcolourblink", 0, .1f);
    }

    public override void exit()
    {
        enemy.fx.Invoke("cancelcolorchange", 0);
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        enemy.zerovelocity();

        if (statetimer < 0f)
            statemachine.changestate(enemy.battlestate);
    }
}
