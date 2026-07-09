using UnityEngine;

public class TimeKingJumpAttackState : EnemyState
{
    private TimeKing enemy;
    private Vector2 landPos;
    private float startX;
    private float moveTimer;
    private float smoothedHorizontalVelocity;
    private bool hitApplied;
    private bool hasLanded;
    private float attackDuration;

    public TimeKingJumpAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.ResetAttackHitTracking();
        enemy.FacePlayer();

        startX = enemy.transform.position.x;
        landPos = enemy.GetJumpLandPosition();
        moveTimer = 0f;
        smoothedHorizontalVelocity = 0f;
        hitApplied = false;
        hasLanded = false;

        attackDuration = enemy.GetAttackDuration(TimeKingAttackType.JumpAttack);
        statetimer = attackDuration;

        enemy.setvelocity(0f, enemy.jumpAttackLaunchY);

        enemy.anim.Play("jump attack", 0, 0f);
        AudioManager.instance.PlaySFX(32, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        moveTimer += Time.deltaTime;
        UpdateJumpMovement();

        if (!hitApplied && moveTimer >= enemy.GetSingleHitTime(TimeKingAttackType.JumpAttack))
            ApplyHit();

        if (!triggercalled && statetimer <= 0f)
            triggercalled = true;

        if (!triggercalled)
            return;

        triggercalled = false;

        if (enemy.TryEnterChaseAfterAttack())
            return;

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

        if (enemy.TryDealSegmentDamage(TimeKingAttackType.JumpAttack))
            hitApplied = true;
    }

    private void UpdateJumpMovement()
    {
        if (hasLanded)
        {
            enemy.zerovelocity();
            return;
        }

        float duration = Mathf.Max(enemy.jumpMoveDuration, 0.05f);
        float progress = Mathf.Clamp01(moveTimer / duration);
        float smoothProgress = progress * progress * (3f - 2f * progress);
        float targetX = Mathf.Lerp(startX, landPos.x, smoothProgress);
        float idealHorizontalVelocity = (targetX - enemy.transform.position.x) / Time.deltaTime;

        float smoothRate = Mathf.Max(enemy.jumpAttackHorizontalSmoothing, 0.01f);
        smoothedHorizontalVelocity = Mathf.Lerp(
            smoothedHorizontalVelocity,
            idealHorizontalVelocity,
            Time.deltaTime * smoothRate);

        enemy.setvelocity(smoothedHorizontalVelocity, rb.velocity.y);

        if (moveTimer > 0.05f && rb.velocity.y <= 0.05f && enemy.isgrounddetected())
        {
            hasLanded = true;
            enemy.transform.position = new Vector3(landPos.x, enemy.transform.position.y, enemy.transform.position.z);
            enemy.zerovelocity();
        }
    }
}
