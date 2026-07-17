using System.Collections.Generic;
using UnityEngine;

public class RootBoss : Enemy
{
    public RootBossIdleState idlestate { get; private set; }
    public RootBossMoveState movestate { get; private set; }
    public RootBossDeadState deadstate { get; private set; }
    public RootBossChangeState changetate { get; private set; }
    public RootBossbattleState battlestate { get; private set; }
    public RootBossAttackState attackstate { get; private set; }
    public RootBossDashState dashstate { get; private set; }
    public RootBossStunnedState stunnedstate { get; private set; }

    private readonly Queue<RootBossAttackType> attackCombo = new Queue<RootBossAttackType>();
    public RootBossAttackType CurrentAttack { get; private set; }

    [Header("战斗")]
    public float battleDetectDistance = 12f;
    public float battleLoseDistance = 20f;

    public bool hasTransformed { get; private set; }

    public bool dashUnlocked => hasTransformed;
    [Header("阶段")]
    public float changeDuration = 1.9f;

    [Header("二阶段冲刺")]
    public float dashspeed = 14f;
    public float dashduration = 0.55f;
    public float dashCooldown = 3.5f;
    public float dashTriggerMinDistance = 1.2f;
    public float dashTriggerMaxDistance = 11f;

    private const float FaceDeadzone = 0.12f;

    [Header("二阶段冲刺伤害")]
    [Tooltip("固定冲撞伤害；为 0 时使用 dashDamageMultiplier × 普通攻击伤害")]
    public int dashFixedDamage = 0;
    [Tooltip("dashFixedDamage 为 0 时生效，在普通攻击伤害上乘此倍率")]
    public float dashDamageMultiplier = 1.5f;
    [Tooltip("冲撞判定半径 = attackcheckradius × 此值")]
    public float dashHitRadiusMultiplier = 1.35f;
    [Tooltip("冲撞判定圆心相对身体的前向偏移（世界单位）")]
    public float dashHitForwardOffset = 1.2f;

    private float lastDashTime = -999f;

    protected override void Awake()
    {
        base.Awake();
        idlestate = new RootBossIdleState(this, statemachine, "Idle", this);
        movestate = new RootBossMoveState(this, statemachine, "Move", this);
        deadstate = new RootBossDeadState(this, statemachine, "Die", this);
        changetate = new RootBossChangeState(this, statemachine, "Change", this);
        battlestate = new RootBossbattleState(this, statemachine, "Move", this);
        attackstate = new RootBossAttackState(this, statemachine, "Attack", this);
        dashstate = new RootBossDashState(this, statemachine, "Dash", this);
        stunnedstate = new RootBossStunnedState(this, statemachine, "Stun", this);
    }

    protected override void Start()
    {
        base.Start();
        statemachine.Initialize(idlestate);
        Stat.onhealthchanged += OnHealthChangedForPhaseTransition;
    }

    private void OnDestroy()
    {
        if (Stat != null)
            Stat.onhealthchanged -= OnHealthChangedForPhaseTransition;
    }

    private void OnHealthChangedForPhaseTransition() => TryStartPhaseTransition();

    public bool IsAboveHalfHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() / 2;

    public bool ShouldTransform() =>
        !hasTransformed && !isDead && Stat.currenthealth > 0 && !IsAboveHalfHealth();

    public bool TryStartPhaseTransition()
    {
        if (!ShouldTransform() || statemachine.currentstate == changetate || statemachine.currentstate == deadstate)
            return false;

        attackCombo.Clear();
        closecounterattackwindow();
        isattack = false;
        AudioManager.instance?.PlaySFX(31, null);
        statemachine.changestate(changetate);
        return true;
    }

    public void BeginAttackCombo(IEnumerable<RootBossAttackType> attacks)
    {
        attackCombo.Clear();
        foreach (RootBossAttackType attack in attacks)
            attackCombo.Enqueue(attack);

        CurrentAttack = attackCombo.Dequeue();
    }

    public bool AdvanceAttackCombo()
    {
        if (attackCombo.Count == 0)
            return false;

        CurrentAttack = attackCombo.Dequeue();
        return true;
    }

    public string GetAttackAnimStateName(RootBossAttackType attackType)
    {
        switch (attackType)
        {
            case RootBossAttackType.Attack2:
                return "RootAttack2";
            case RootBossAttackType.Attack4:
                return "RootAttack4";
            default:
                return "RootAttack1";
        }
    }

    public float GetAttackDuration(RootBossAttackType attackType)
    {
        switch (attackType)
        {
            case RootBossAttackType.Attack2:
                return 1f;
            case RootBossAttackType.Attack4:
                return 1.05f;
            default:
                return 1f;
        }
    }

    /// <summary>Attack2/4 在 Animator 里没有出口，必须从代码切回行走/待机，否则会定格在最后一帧。</summary>
    public void SyncBattleAnimator(bool walking)
    {
        anim.SetBool("Attack", false);
        anim.SetBool("Dash", false);
        anim.SetBool("Change", false);
        anim.SetBool("Stun", false);
        anim.SetBool("Move", walking);
        anim.Play(walking ? "RootWalk" : "RootIdle", 0, 0f);
    }

    public bool IsAttackReady() => Time.time >= lasttimeattack + attackcooldown;

    public void ConsumeAttackCooldown()
    {
        attackcooldown = Random.Range(minattackcooldown, maxattackcooldown);
        lasttimeattack = Time.time;
    }

    public void OnTransformComplete()
    {
        hasTransformed = true;
        // 变身结束立刻允许一次冲撞，不被变身前的普攻冷却拖住
        lastDashTime = -999f;
    }

    public bool CanDash() => dashUnlocked && Time.time >= lastDashTime + dashCooldown;

    public void MarkDashUsed() => lastDashTime = Time.time;

    public void FacePlayer(float playerX)
    {
        float unused = 0f;
        FacePlayer(playerX, ref unused, 0f);
    }

    public void FacePlayer(float playerX, ref float flipCooldown, float flipDelay)
    {
        if (IsFacingLocked)
            return;

        if (flipCooldown > 0f)
        {
            flipCooldown -= Time.deltaTime;
            return;
        }

        float dx = playerX - transform.position.x;
        if (dx > FaceDeadzone && !faceright)
        {
            Flip();
            flipCooldown = flipDelay;
        }
        else if (dx < -FaceDeadzone && faceright)
        {
            Flip();
            flipCooldown = flipDelay;
        }
    }

    public bool IsDashInRange(Vector2 playerPosition)
    {
        float dist = Vector2.Distance(transform.position, playerPosition);
        return dist >= dashTriggerMinDistance && dist <= dashTriggerMaxDistance;
    }

    public override RaycastHit2D ispalyerdetected()
    {
        if (wallcheck == null)
            return default;

        // 检测点在身体负局部 X，Y180 翻转后与朝向同侧；双向射线避免短暂朝向不同步丢索敌
        RaycastHit2D forward = Physics2D.Raycast(wallcheck.position, Vector2.right * facedir, 20f, whatisplayer);
        if (forward.collider != null)
            return forward;

        return Physics2D.Raycast(wallcheck.position, Vector2.left * facedir, 20f, whatisplayer);
    }

    public bool TryDealDashDamage()
    {
        // 用攻击检测点更稳：该 prefab 的检测点在朝向一侧（配合根节点 Y180）
        Vector2 dashHitCenter = attackcheck != null
            ? (Vector2)attackcheck.position
            : rb.position + new Vector2(facedir * dashHitForwardOffset, 0.05f);

        if (dashFixedDamage > 0)
        {
            return DealDamageToDetectedPlayers(
                dashHitRadiusMultiplier,
                dashFixedDamage,
                useSharedHitFrameGuard: false,
                worldCenterOverride: dashHitCenter);
        }

        return DealDamageToDetectedPlayers(
            dashHitRadiusMultiplier,
            damageMultiplier: dashDamageMultiplier,
            useSharedHitFrameGuard: false,
            worldCenterOverride: dashHitCenter);
    }

    public override bool canbestun()
    {
        if (ShouldTransform())
        {
            TryStartPhaseTransition();
            return false;
        }

        if (base.canbestun() && !isDead)
        {
            statemachine.changestate(stunnedstate);
            return true;
        }
        return false;
    }

    public override void Die()
    {
        base.Die();
        statemachine.changestate(deadstate);
    }
}
