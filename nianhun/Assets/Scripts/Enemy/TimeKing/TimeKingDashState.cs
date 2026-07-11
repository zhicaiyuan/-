using UnityEngine;

public class TimeKingDashState : EnemyState
{
    private enum DashPhase
    {
        DashOut,
        Warning,
        DashIn
    }

    private TimeKing enemy;
    private DashPhase phase;
    private Vector2 landPosition;
    private GameObject warningObject;
    private float phaseElapsed;
    private bool hitApplied;
    private bool visualsHidden;

    public TimeKingDashState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
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
        enemy.ResetAttackHitTracking();
        enemy.FacePlayer();

        phase = DashPhase.DashOut;
        phaseElapsed = 0f;
        hitApplied = false;
        landPosition = enemy.GetDashLandPosition();
        warningObject = null;
        visualsHidden = false;

        // 先播 dash out，播完再隐藏（否则看不到动画）
        enemy.SetCombatVisible(true);
        enemy.anim.Play("dash out", 0, 0f);
        statetimer = enemy.dashOutDuration;
        AudioManager.instance.PlaySFX(32, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
        CleanupWarning();
        if (visualsHidden)
            enemy.SetCombatVisible(true);
        if (enemy.Stat.isInvincible && enemy.statemachine.currentstate != enemy.changestate)
            enemy.Stat.MakeInvincible(false);
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        enemy.zerovelocity();
        phaseElapsed += Time.deltaTime;

        switch (phase)
        {
            case DashPhase.DashOut:
                if (triggercalled || phaseElapsed >= enemy.dashOutDuration)
                    EnterWarningPhase();
                break;

            case DashPhase.Warning:
                landPosition = enemy.GetDashLandPosition();
                if (warningObject != null)
                    warningObject.transform.position = new Vector3(landPosition.x, landPosition.y, 0f);

                if (phaseElapsed >= enemy.dashWarningDuration)
                    EnterDashInPhase();
                break;

            case DashPhase.DashIn:
                if (!hitApplied && phaseElapsed >= enemy.dashHitTime)
                    ApplyDashHit();

                if (triggercalled || phaseElapsed >= enemy.dashInDuration)
                    FinishDash();
                break;
        }
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }

    public void ApplyDashHit()
    {
        if (hitApplied)
            return;

        if (enemy.TryDealSegmentDamage(TimeKingAttackType.Dash))
            hitApplied = true;
    }

    private void EnterWarningPhase()
    {
        phase = DashPhase.Warning;
        phaseElapsed = 0f;
        triggercalled = false;

        enemy.Stat.MakeInvincible(true);
        enemy.SetCombatVisible(false);
        visualsHidden = true;

        landPosition = enemy.GetDashLandPosition();
        CleanupWarning();
        warningObject = TimeKingAttackWarning.Show(
            landPosition,
            enemy.dashWarningSize,
            enemy.dashWarningDuration + 0.15f,
            enemy.warningColor);
    }

    private void EnterDashInPhase()
    {
        phase = DashPhase.DashIn;
        phaseElapsed = 0f;
        triggercalled = false;
        hitApplied = false;
        CleanupWarning();

        enemy.transform.position = new Vector3(landPosition.x, landPosition.y, enemy.transform.position.z);
        enemy.SetCombatVisible(true);
        visualsHidden = false;
        enemy.Stat.MakeInvincible(false);
        enemy.FacePlayer();
        enemy.anim.Play("dash in", 0, 0f);
        AudioManager.instance.PlaySFX(30, null);
    }

    private void FinishDash()
    {
        if (!hitApplied)
            ApplyDashHit();

        if (enemy.TryEnterChaseAfterAttack())
            return;

        if (enemy.AdvanceAttackCombo())
            enemy.EnterAttackForCurrentSkill();
        else
            statemachine.changestate(enemy.recoverystate);
    }

    private void CleanupWarning()
    {
        if (warningObject == null)
            return;

        Object.Destroy(warningObject);
        warningObject = null;
    }
}
