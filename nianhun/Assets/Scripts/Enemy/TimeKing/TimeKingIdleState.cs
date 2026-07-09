using UnityEngine;

public class TimeKingIdleState : EnemyState
{
    private TimeKing enemy;

    public TimeKingIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.Stat.MakeInvincible(false);
        enemy.zerovelocity();
        enemy.SyncGroundAnimator(false);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (!enemy.IsPlayerInBattleRange())
            return;

        enemy.MarkEnteredBattle();
        enemy.FacePlayer();
        statemachine.changestate(enemy.battlestate);
    }
}
