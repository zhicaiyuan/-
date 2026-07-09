using UnityEngine;

public class TimeKingRecoveryState : EnemyState
{
    private TimeKing enemy;

    public TimeKingRecoveryState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.zerovelocity();
        enemy.MarkRecoveryStarted();
        statetimer = enemy.recoveryDuration;
        enemy.SyncGroundAnimator(false);
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
