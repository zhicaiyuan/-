using UnityEngine;

public class WalkingStickAttackState : EnemyState
{
    private WalkingStick enemy;
    private bool damageApplied;
    private float attackDuration;
    private float damageAtTimer;

    public WalkingStickAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, WalkingStick walkingStick)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = walkingStick;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.zerovelocity();
        enemy.FacePlayer();

        damageApplied = false;
        attackDuration = enemy.GetAttackDuration(enemy.CurrentAttack);
        statetimer = attackDuration;
        damageAtTimer = attackDuration - enemy.GetAttackHitTime(enemy.CurrentAttack);

        enemy.anim.SetBool("idle", false);
        enemy.anim.SetBool("move", false);
        enemy.anim.SetBool("attack", false);
        enemy.anim.Play(enemy.GetAttackAnimStateName(enemy.CurrentAttack), 0, 0f);

        if (enemy.CurrentAttack == WalkingStickAttackType.Attack1)
            enemy.MarkAttack1Used();

        AudioManager.instance.PlaySFX(5, null);
    }

    public override void exit()
    {
        enemybase.anim.SetBool(animboolname, false);
        enemybase.AssignlastAnimName(animboolname);
        enemy.isattack = false;
        enemy.lasttimeattack = Time.time;
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (!damageApplied && statetimer <= damageAtTimer)
        {
            enemy.DealAttackDamage(enemy.CurrentAttack);
            damageApplied = true;
        }

        if (!triggercalled && statetimer <= 0f)
            triggercalled = true;

        if (!triggercalled)
            return;

        triggercalled = false;

        if (enemy.IsPhase2)
            statemachine.changestate(enemy.walkstate);
        else
            statemachine.changestate(enemy.idlestate);
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;

        if (!damageApplied)
        {
            enemy.DealAttackDamage(enemy.CurrentAttack);
            damageApplied = true;
        }
    }
}
