using UnityEngine;

public class TimeKingAttackState : EnemyState
{
    private TimeKing enemy;
    private bool[] segmentHitApplied;
    private bool singleHitApplied;
    private float attackElapsed;

    public TimeKingAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.zerovelocity();
        enemy.FacePlayer();
        enemy.ResetAttackHitTracking();
        PlayCurrentAttack();
    }

    public override void exit()
    {
        enemy.isattack = false;
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        enemy.zerovelocity();

        attackElapsed += Time.deltaTime;
        TryApplyTimedHits();

        if (!triggercalled && statetimer <= 0f)
            triggercalled = true;

        if (!triggercalled)
            return;

        triggercalled = false;

        if (enemy.TryEnterChaseAfterAttack())
            return;

        if (enemy.AdvanceAttackCombo())
            PlayCurrentAttack();
        else
            statemachine.changestate(enemy.recoverystate);
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }

    public void ApplySegmentHit(int segmentIndex)
    {
        if (!enemy.IsMultiHitAttack(enemy.CurrentAttack))
            return;

        if (segmentHitApplied == null || segmentIndex < 0 || segmentIndex >= segmentHitApplied.Length)
            return;

        if (segmentHitApplied[segmentIndex])
            return;

        if (enemy.TryDealSegmentDamage(enemy.CurrentAttack, segmentIndex))
            segmentHitApplied[segmentIndex] = true;
    }

    public void ApplySingleHit()
    {
        if (enemy.IsMultiHitAttack(enemy.CurrentAttack) || singleHitApplied)
            return;

        if (enemy.TryDealSegmentDamage(enemy.CurrentAttack))
            singleHitApplied = true;
    }

    private void TryApplyTimedHits()
    {
        if (enemy.IsMultiHitAttack(enemy.CurrentAttack))
        {
            TimeKingHitSegment[] segments = enemy.GetMultiHitSegments(enemy.CurrentAttack);
            if (segments == null || segmentHitApplied == null)
                return;

            int count = Mathf.Min(segments.Length, segmentHitApplied.Length);
            for (int i = 0; i < count; i++)
            {
                if (segmentHitApplied[i])
                    continue;

                if (attackElapsed >= segments[i].hitTime)
                    ApplySegmentHit(i);
            }

            return;
        }

        if (!singleHitApplied && attackElapsed >= enemy.GetSingleHitTime(enemy.CurrentAttack))
            ApplySingleHit();
    }

    public void PlayCurrentAttack()
    {
        triggercalled = false;
        enemy.ResetAttackHitTracking();
        attackElapsed = 0f;
        statetimer = enemy.GetAttackDuration(enemy.CurrentAttack);
        singleHitApplied = false;

        if (enemy.IsMultiHitAttack(enemy.CurrentAttack))
        {
            TimeKingHitSegment[] segments = enemy.GetMultiHitSegments(enemy.CurrentAttack);
            segmentHitApplied = new bool[segments != null ? segments.Length : 0];
        }
        else
        {
            segmentHitApplied = null;
        }

        enemy.anim.Play(enemy.GetAttackAnimStateName(enemy.CurrentAttack), 0, 0f);
        AudioManager.instance.PlaySFX(30, null);
    }
}
