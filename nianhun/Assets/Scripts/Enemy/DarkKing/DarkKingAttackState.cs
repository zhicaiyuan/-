using UnityEngine;

public class DarkKingAttackState : EnemyState
{
    private DarkKing enemy;
    private float elapsed;
    private bool hitApplied;
    private bool counterOpened;
    private bool counterClosed;

    public DarkKingAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.zerovelocity();
        enemy.FacePlayer();
        enemy.ResetAttackHitTracking();
        enemy.closecounterattackwindow();

        elapsed = 0f;
        hitApplied = false;
        counterOpened = false;
        counterClosed = false;
        statetimer = enemy.attackDuration;
        enemy.anim.Play("attack", 0, 0f);
        AudioManager.instance.PlaySFX(37, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.closecounterattackwindow();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();
        elapsed += Time.deltaTime;

        float openAt = Mathf.Max(0f, enemy.attackHitTime - enemy.attackCounterOpenBeforeHit);
        float closeAt = enemy.attackHitTime + enemy.attackCounterCloseAfterHit;

        if (!counterOpened && elapsed >= openAt)
        {
            counterOpened = true;
            enemy.opencounterattackwindow();
        }

        if (counterOpened && !counterClosed && elapsed >= closeAt)
        {
            counterClosed = true;
            enemy.closecounterattackwindow();
        }

        if (!hitApplied && elapsed >= enemy.attackHitTime)
            ApplyHit();

        if (!triggercalled && statetimer <= 0f)
            triggercalled = true;

        if (!triggercalled)
            return;

        triggercalled = false;
        statemachine.changestate(enemy.recoverystate);
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }

    public void ApplyHit()
    {
        if (hitApplied)
            return;

        hitApplied = true;
        enemy.TryDealAttackDamage();
    }
}
