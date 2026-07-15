using UnityEngine;

public class DarkKingTeleportState : EnemyState
{
    private enum Phase
    {
        Transport,
        Warning,
        Appear
    }

    private DarkKing enemy;
    private Phase phase;
    private Vector2 landPosition;
    private GameObject warningObject;
    private float phaseElapsed;
    private bool hitApplied;
    private bool visualsHidden;

    public DarkKingTeleportState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
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
        enemy.ResetAttackHitTracking();
        enemy.FacePlayer();

        phase = Phase.Transport;
        phaseElapsed = 0f;
        hitApplied = false;
        visualsHidden = false;
        warningObject = null;
        landPosition = enemy.GetTeleportLandPosition();

        enemy.SetCombatVisible(true);
        enemy.anim.Play("transport", 0, 0f);
        statetimer = enemy.teleportOutDuration;
        AudioManager.instance.PlaySFX(36, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
        CleanupWarning();
        if (visualsHidden)
            enemy.SetCombatVisible(true);
        if (enemy.Stat.isInvincible)
            enemy.Stat.MakeInvincible(false);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();
        phaseElapsed += Time.deltaTime;

        switch (phase)
        {
            case Phase.Transport:
                if (triggercalled || phaseElapsed >= enemy.teleportOutDuration)
                    EnterWarning();
                break;

            case Phase.Warning:
                landPosition = enemy.GetTeleportLandPosition();
                if (warningObject != null)
                    warningObject.transform.position = new Vector3(landPosition.x, landPosition.y, 0f);

                if (phaseElapsed >= enemy.teleportWarningDuration)
                    EnterAppear();
                break;

            case Phase.Appear:
                if (!hitApplied && phaseElapsed >= enemy.teleportHitTime)
                    ApplyHit();

                if (triggercalled || phaseElapsed >= enemy.teleportInDuration)
                {
                    if (!hitApplied)
                        ApplyHit();
                    statemachine.changestate(enemy.recoverystate);
                }
                break;
        }
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
        enemy.TryDealTeleportDamage();
    }

    private void EnterWarning()
    {
        phase = Phase.Warning;
        phaseElapsed = 0f;
        triggercalled = false;

        enemy.Stat.MakeInvincible(true);
        enemy.SetCombatVisible(false);
        visualsHidden = true;

        landPosition = enemy.GetTeleportLandPosition();
        CleanupWarning();
        warningObject = DarkKingAttackWarning.Show(
            landPosition,
            enemy.teleportWarningSize,
            enemy.teleportWarningDuration + 0.15f,
            enemy.warningColor);
    }

    private void EnterAppear()
    {
        phase = Phase.Appear;
        phaseElapsed = 0f;
        triggercalled = false;
        hitApplied = false;
        CleanupWarning();

        Vector2 land = enemy.GetTeleportLandPosition();
        land.y = enemy.GetCombatGroundY();
        enemy.transform.position = new Vector3(land.x, land.y, enemy.transform.position.z);
        enemy.SetCombatVisible(true);
        visualsHidden = false;
        enemy.Stat.MakeInvincible(false);
        enemy.FacePlayer();
        enemy.anim.Play("appear", 0, 0f);
        AudioManager.instance.PlaySFX(30, null);
    }

    private void CleanupWarning()
    {
        if (warningObject == null)
            return;

        Object.Destroy(warningObject);
        warningObject = null;
    }
}
