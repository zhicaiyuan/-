using UnityEngine;

public class TimeKingEnterState : EnemyState
{
    private TimeKing enemy;

    public TimeKingEnterState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isUnstoppable = false;
        enemy.Stat.MakeInvincible(true);
        enemy.zerovelocity();
        statetimer = enemy.enterDuration;
        enemy.anim.Play("enter", 0, 0f);
    }

    public override void exit()
    {
        enemy.isUnstoppable = false;
        enemy.Stat.MakeInvincible(false);
        enemy.OnEnterPresentationComplete();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled || statetimer < 0f)
            statemachine.changestate(enemy.idlestate);
    }

    public override void aniamtionfinishtrigger()
    {
        enemy.Stat.MakeInvincible(false);
        triggercalled = true;
    }
}
