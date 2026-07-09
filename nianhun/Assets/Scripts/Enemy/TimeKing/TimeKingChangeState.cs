using UnityEngine;

public class TimeKingChangeState : EnemyState
{
    private TimeKing enemy;

    public TimeKingChangeState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.zerovelocity();
        enemy.isattack = false;
        enemy.ClearAttackCombo();
        enemy.closecounterattackwindow();
        enemy.Stat.MakeInvincible(true);
        statetimer = enemy.changeDuration;
        enemy.anim.Play("change", 0, 0f);
    }

    public override void exit()
    {
        enemy.Stat.MakeInvincible(false);
        enemy.OnTransformComplete();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled || statetimer < 0f)
            statemachine.changestate(enemy.battlestate);
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }
}
