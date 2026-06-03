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
    public float dashduration = 0.45f;
    public float dashCooldown = 3f;
    public float dashTriggerMinDistance = 2.5f;
    public float dashTriggerMaxDistance = 9f;

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
    }

    public bool IsAboveHalfHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() / 2;

    public bool ShouldTransform() => !hasTransformed && !isDead && !IsAboveHalfHealth();

    public bool CanEnterPhaseTransition(EnemyState state) =>
        state != changetate && state != deadstate && state != stunnedstate && state != dashstate && state != attackstate;

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

    public void TryStartPhaseTransition()
    {
        if (!ShouldTransform() || !CanEnterPhaseTransition(statemachine.currentstate))
            return;
        AudioManager.instance.PlaySFX(31, null);
        statemachine.changestate(changetate);
    }

    public void OnTransformComplete() => hasTransformed = true;

    public bool CanDash() => dashUnlocked && Time.time >= lastDashTime + dashCooldown;

    public void MarkDashUsed() => lastDashTime = Time.time;

    public bool IsDashInRange(Vector2 playerPosition)
    {
        float dist = Vector2.Distance(transform.position, playerPosition);
        return dist >= dashTriggerMinDistance && dist <= dashTriggerMaxDistance;
    }

    public override bool canbestun()
    {
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
