using UnityEngine;

public class RootBossAttackState : EnemyState
{
    private RootBoss enemy;

    public RootBossAttackState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.zerovelocity();
        PlayCurrentAttack();
    }

    public override void exit()
    {
        enemybase.anim.SetBool(animboolname, false);
        enemybase.AssignlastAnimName(animboolname);
        enemy.isattack = false;
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (enemy.TryStartPhaseTransition())
            return;

        if (!triggercalled && statetimer < 0f)
            triggercalled = true;

        if (!triggercalled)
            return;

        triggercalled = false;

        if (enemy.AdvanceAttackCombo())
            PlayCurrentAttack();
        else
            statemachine.changestate(enemy.battlestate);
    }

    private void PlayCurrentAttack()
    {
        statetimer = enemy.GetAttackDuration(enemy.CurrentAttack);

        enemybase.anim.SetBool("Move", false);
        enemybase.anim.SetBool("Attack", false);
        enemybase.anim.SetBool("Dash", false);
        AudioManager.instance.PlaySFX(30, null);
        enemybase.anim.Play(enemy.GetAttackAnimStateName(enemy.CurrentAttack), 0, 0f);
    }
}
