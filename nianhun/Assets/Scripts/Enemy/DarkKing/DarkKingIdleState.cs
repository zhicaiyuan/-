using UnityEngine;

public class DarkKingIdleState : EnemyState
{
    private DarkKing enemy;

    public DarkKingIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
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
