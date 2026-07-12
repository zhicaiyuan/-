using UnityEngine;

public class DarkKingRecoveryState : EnemyState
{
    private DarkKing enemy;

    public DarkKingRecoveryState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
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
        enemy.zerovelocity();

        if (statetimer < 0f)
            statemachine.changestate(enemy.battlestate);
    }
}
