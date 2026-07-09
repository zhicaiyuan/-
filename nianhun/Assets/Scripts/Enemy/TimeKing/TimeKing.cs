using System.Collections.Generic;
using UnityEngine;

public class TimeKing : Enemy
{
    public TimeKingEnterState enterstate { get; private set; }
    public TimeKingIdleState idlestate { get; private set; }
    public TimeKingBattleState battlestate { get; private set; }
    public TimeKingAttackState attackstate { get; private set; }
    public TimeKingJumpAttackState jumpattackstate { get; private set; }
    public TimeKingRecoveryState recoverystate { get; private set; }
    public TimeKingChaseState chasestate { get; private set; }
    public TimeKingChangeState changestate { get; private set; }
    public TimeKingStunnedState stunnedstate { get; private set; }
    public TimeKingDeadState deadstate { get; private set; }

    private readonly Queue<TimeKingAttackType> attackCombo = new Queue<TimeKingAttackType>();
    private readonly Dictionary<TimeKingAttackType, float> lastSkillUsedTime = new Dictionary<TimeKingAttackType, float>();

    public TimeKingAttackType CurrentAttack { get; set; }

    private readonly HashSet<int> appliedAttackHitKeys = new HashSet<int>();
    public bool HasEnteredBattle { get; private set; }
    public bool IsPhase2 { get; private set; }
    private bool hasTransformed;

    [Header("时之王 战斗")]
    public float battleDetectDistance = 12f;
    public float battleLoseDistance = 20f;
    public float jumpAttackDistance = 10f;
    public float jumpLandOffset = 1.5f;
    public float jumpMoveDuration = 0.55f;
    [Tooltip("跳劈起跳时的竖直速度")]
    public float jumpAttackLaunchY = 12f;
    [Tooltip("跳劈水平速度跟随平滑系数，越大越快到位")]
    public float jumpAttackHorizontalSmoothing = 10f;
    public float recoveryDuration = 1.2f;
    public float enterDuration = 3f;
    public float minGapBetweenCombos = 0.2f;
    public float changeDuration = 2f;

    [Header("二阶段追击")]
    [Tooltip("当前招式结束后，玩家距离超过该值则中断连招并追击")]
    public float postAttackChaseDistance = 6f;
    [Tooltip("贴身时的追击速度（距离近）")]
    public float chaseSpeedNear = 3f;
    [Tooltip("拉远时的追击速度（距离远）")]
    public float chaseSpeedFar = 10f;
    [Tooltip("达到该距离时速度拉满到 chaseSpeedFar；近端用 attackcheckdistance")]
    public float chaseSpeedFarDistance = 12f;
    [Tooltip("追击速度随距离变化的平滑速度，越大跟得越紧")]
    public float chaseSpeedSmoothRate = 8f;

    public float GetChaseSpeedForDistance(float distanceToPlayer)
    {
        float nearDistance = Mathf.Max(0.01f, attackcheckdistance);
        float farDistance = Mathf.Max(nearDistance + 0.01f, chaseSpeedFarDistance);
        float t = Mathf.InverseLerp(nearDistance, farDistance, distanceToPlayer);
        t = t * t * (3f - 2f * t); // smoothstep
        return Mathf.Lerp(chaseSpeedNear, chaseSpeedFar, t);
    }

    [Header("招式 CD")]
    public float attack1Cooldown = 3f;
    public float attack2Cooldown = 4f;
    public float attack3Cooldown = 5f;
    public float attack4Cooldown = 6f;
    public float attack5Cooldown = 5f;
    public float attack6Cooldown = 5f;
    public float attack7Cooldown = 5f;
    public float jumpAttackCooldown = 8f;
    [Tooltip("二阶段时 Attack1–4 在原 CD 上额外增加的冷却")]
    public float phase2Attack1234ExtraCooldown = 6f;

    [Header("Attack1 多段")]
    [SerializeField] private TimeKingHitSegment[] attack1Segments;
    public float attack1Duration = 2f;

    [Header("Attack3 多段")]
    [SerializeField] private TimeKingHitSegment[] attack3Segments;
    public float attack3Duration = 2.8f;

    [Header("调试 Gizmo")]
    [Tooltip("关闭后隐藏所有招式判定点 Gizmo，避免 Scene 视图过于杂乱")]
    public bool showAttackRangeGizmos = true;

    [Header("Attack2 单段")]
    [SerializeField] private TimeKingHitArea attack2Hit;
    public float attack2Duration = 1.2f;
    [Tooltip("从攻击开始到出伤的时间（秒），与动画事件二选一或并存，以先到为准且只结算一次")]
    public float attack2HitTime = 0.6f;

    [Header("Attack4 多段")]
    [SerializeField] private TimeKingHitSegment[] attack4Segments;
    public float attack4Duration = 1.5f;

    [Header("Attack5 多段")]
    [SerializeField] private TimeKingHitSegment[] attack5Segments;
    public float attack5Duration = 2.2f;

    [Header("Attack6 多段")]
    [SerializeField] private TimeKingHitSegment[] attack6Segments;
    public float attack6Duration = 1.6f;

    [Header("Attack7 多段")]
    [SerializeField] private TimeKingHitSegment[] attack7Segments;
    public float attack7Duration = 1.6f;

    [Header("JumpAttack 判定")]
    [SerializeField] private TimeKingHitArea jumpAttackHit;
    public float jumpAttackAnimDuration = 1.9f;
    [Tooltip("从跳劈开始到出伤的时间（秒）")]
    public float jumpAttackHitTime = 1f;

    private float lastComboRecoveryEndTime;
    private float lastFlipTime;

    private const float FaceDeadzone = 0.2f;
    private const float FlipCooldown = 0.25f;

    private Transform worldHealthBarRoot;

    protected override void Awake()
    {
        base.Awake();
        EnsureDefaultHitSegments();
        worldHealthBarRoot = transform.Find("entity-stat-UI");
        if (worldHealthBarRoot != null)
            worldHealthBarRoot.gameObject.SetActive(false);
        enterstate = new TimeKingEnterState(this, statemachine, "TimeKingEnter", this);
        idlestate = new TimeKingIdleState(this, statemachine, "TimeKingIdle", this);
        battlestate = new TimeKingBattleState(this, statemachine, "TimeKingBattle", this);
        attackstate = new TimeKingAttackState(this, statemachine, "TimeKingAttack", this);
        jumpattackstate = new TimeKingJumpAttackState(this, statemachine, "TimeKingJumpAttack", this);
        recoverystate = new TimeKingRecoveryState(this, statemachine, "TimeKingRecovery", this);
        chasestate = new TimeKingChaseState(this, statemachine, "TimeKingChase", this);
        changestate = new TimeKingChangeState(this, statemachine, "TimeKingChange", this);
        stunnedstate = new TimeKingStunnedState(this, statemachine, "TimeKingStunned", this);
        deadstate = new TimeKingDeadState(this, statemachine, "TimeKingDead", this);
    }

    protected override void Start()
    {
        base.Start();
        Stat.onhealthchanged += OnHealthChangedForPhaseTransition;
        if (!isDead)
            statemachine.Initialize(enterstate);
    }

    private void OnDestroy()
    {
        if (Stat != null)
            Stat.onhealthchanged -= OnHealthChangedForPhaseTransition;
    }

    protected override void Update()
    {
        base.Update();
        EnsureVulnerableOutsideProtectedStates();

        if (isknocked && !isattack && !isDead
            && statemachine.currentstate != enterstate
            && statemachine.currentstate != changestate)
            statemachine.changestate(stunnedstate);
    }

    private void EnsureVulnerableOutsideProtectedStates()
    {
        if (isDead
            || statemachine.currentstate == enterstate
            || statemachine.currentstate == changestate)
            return;

        if (Stat.isInvincible)
            Stat.MakeInvincible(false);
    }

    private void OnHealthChangedForPhaseTransition() => TryStartPhaseTransition();

    public bool IsAboveTwoThirdsHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() * 2f / 3f;

    public bool ShouldTransform() =>
        !hasTransformed && !isDead && Stat.currenthealth > 0 && !IsAboveTwoThirdsHealth();

    public bool TryStartPhaseTransition()
    {
        if (!ShouldTransform()
            || statemachine.currentstate == changestate
            || statemachine.currentstate == deadstate)
            return false;

        ClearAttackCombo();
        closecounterattackwindow();
        isattack = false;
        AudioManager.instance.PlaySFX(31, null);
        statemachine.changestate(changestate);
        return true;
    }

    public void OnTransformComplete()
    {
        hasTransformed = true;
        IsPhase2 = true;
    }

    public void MarkEnteredBattle() => HasEnteredBattle = true;

    public void OnEnterPresentationComplete()
    {
        Stat.MakeInvincible(false);

        if (worldHealthBarRoot != null)
            worldHealthBarRoot.gameObject.SetActive(false);

        BossScreenHealthBar.Show(Stat);
    }

    public void ResetAttackHitTracking() => appliedAttackHitKeys.Clear();

    public void ClearAttackCombo() => attackCombo.Clear();

    public bool ShouldChaseAfterAttack()
    {
        if (!IsPhase2 || isDead)
            return false;

        return GetDistanceToPlayer() > postAttackChaseDistance;
    }

    public bool TryEnterChaseAfterAttack()
    {
        if (!ShouldChaseAfterAttack())
            return false;

        ClearAttackCombo();
        statemachine.changestate(chasestate);
        return true;
    }

    public bool TryDealSegmentDamage(TimeKingAttackType attackType, int segmentIndex = 0)
    {
        int key = ((int)attackType << 8) | segmentIndex;
        if (!appliedAttackHitKeys.Add(key))
            return false;

        DealAttackDamage(attackType, segmentIndex);
        return true;
    }

    public float GetSingleHitTime(TimeKingAttackType attackType)
    {
        switch (attackType)
        {
            case TimeKingAttackType.JumpAttack:
                return jumpAttackHitTime;
            default:
                return attack2HitTime;
        }
    }

    private void EnsureDefaultHitSegments()
    {
        if (attack1Segments == null || attack1Segments.Length == 0)
        {
            attack1Segments = new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1f, hitTime = 0.5f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.05f, hitTime = 1f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.1f, hitTime = 1.5f, damageMultiplier = 1f }
            };
        }

        if (attack3Segments == null || attack3Segments.Length == 0)
        {
            attack3Segments = new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.1f, hitTime = 0.8f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.15f, hitTime = 1.5f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.2f, hitTime = 2.2f, damageMultiplier = 1f }
            };
        }

        if (attack2Hit == null || attack2Hit.hitCheck == null)
            attack2Hit = CreateDefaultHitArea(attackcheck, 1.1f);

        if (attack4Segments == null || attack4Segments.Length == 0)
        {
            attack4Segments = new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.2f, hitTime = 0.5f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.25f, hitTime = 0.9f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.3f, hitTime = 1.3f, damageMultiplier = 1f }
            };
        }

        attack5Segments = EnsureSegments(
            attack5Segments,
            new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.15f, hitTime = 0.55f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.2f, hitTime = 1.1f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.25f, hitTime = 1.65f, damageMultiplier = 1f }
            });

        attack6Segments = EnsureSegments(
            attack6Segments,
            new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.2f, hitTime = 0.5f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.25f, hitTime = 1.1f, damageMultiplier = 1f }
            });

        attack7Segments = EnsureSegments(
            attack7Segments,
            new[]
            {
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.2f, hitTime = 0.5f, damageMultiplier = 1f },
                new TimeKingHitSegment { hitCheck = attackcheck, radius = 1.3f, hitTime = 1.1f, damageMultiplier = 1f }
            });

        if (jumpAttackHit == null || jumpAttackHit.hitCheck == null)
            jumpAttackHit = CreateDefaultHitArea(attackcheck, 1.2f);

        RepairHitChecks(attack1Segments);
        RepairHitChecks(attack3Segments);
        RepairHitChecks(attack4Segments);
        RepairHitChecks(attack5Segments);
        RepairHitChecks(attack6Segments);
        RepairHitChecks(attack7Segments);
        if (attack2Hit != null && attack2Hit.hitCheck == null)
            attack2Hit.hitCheck = attackcheck;
        if (jumpAttackHit != null && jumpAttackHit.hitCheck == null)
            jumpAttackHit.hitCheck = attackcheck;
    }

    private TimeKingHitSegment[] EnsureSegments(TimeKingHitSegment[] current, TimeKingHitSegment[] defaults)
    {
        if (current == null || current.Length == 0)
            return defaults;

        return current;
    }

    private void RepairHitChecks(TimeKingHitSegment[] segments)
    {
        if (segments == null || attackcheck == null)
            return;

        foreach (TimeKingHitSegment segment in segments)
        {
            if (segment != null && segment.hitCheck == null)
                segment.hitCheck = attackcheck;
        }
    }

    private static TimeKingHitArea CreateDefaultHitArea(Transform check, float radius)
    {
        return new TimeKingHitArea
        {
            hitCheck = check,
            shape = TimeKingHitShape.Circle,
            radius = radius,
            boxSize = new Vector2(radius * 2f, radius * 2f)
        };
    }

    public bool IsPlayerInBattleRange()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        Transform player = playermanger.instance.player.transform;
        if (Vector2.Distance(transform.position, player.position) <= battleDetectDistance)
            return true;

        return ispalyerdetected().collider != null;
    }

    public float GetDistanceToPlayer()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return float.MaxValue;

        return Vector2.Distance(transform.position, playermanger.instance.player.transform.position);
    }

    public void FacePlayer()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        if (Time.time < lastFlipTime + FlipCooldown)
            return;

        float dx = playermanger.instance.player.transform.position.x - transform.position.x;
        if (dx > FaceDeadzone && !faceright)
        {
            Flip();
            lastFlipTime = Time.time;
        }
        else if (dx < -FaceDeadzone && faceright)
        {
            Flip();
            lastFlipTime = Time.time;
        }
    }

    public override void damage(float attackdirx)
    {
        if (isDead
            || statemachine.currentstate == enterstate
            || statemachine.currentstate == changestate
            || Stat.isInvincible)
            return;

        AudioManager.instance.PlaySFX(7, null);
        fx.StartCoroutine("flashfx");
    }

    public bool IsComboReady() => Time.time >= lastComboRecoveryEndTime + minGapBetweenCombos;

    public void MarkRecoveryStarted() => lastComboRecoveryEndTime = Time.time + recoveryDuration;

    public bool IsSkillReady(TimeKingAttackType attackType)
    {
        if (!IsComboReady())
            return false;

        return Time.time >= GetLastSkillUsedTime(attackType) + GetSkillCooldown(attackType);
    }

    public bool IsComboExecutable(IEnumerable<TimeKingAttackType> combo)
    {
        if (!IsComboReady() || combo == null)
            return false;

        // 只卡连招第一招 CD；后续招式在 Advance 时再消耗，避免 2-6 / 3-2-6 因 Attack2 长 CD 永远选不中
        foreach (TimeKingAttackType attack in combo)
            return IsSkillReady(attack);

        return false;
    }

    public void ConsumeSkillCooldown(TimeKingAttackType attackType)
    {
        lastSkillUsedTime[attackType] = Time.time;
    }

    private float GetLastSkillUsedTime(TimeKingAttackType attackType)
    {
        return lastSkillUsedTime.TryGetValue(attackType, out float time) ? time : -999f;
    }

    public float GetSkillCooldown(TimeKingAttackType attackType)
    {
        float cooldown;
        switch (attackType)
        {
            case TimeKingAttackType.Attack2:
                cooldown = attack2Cooldown;
                break;
            case TimeKingAttackType.Attack3:
                cooldown = attack3Cooldown;
                break;
            case TimeKingAttackType.Attack4:
                cooldown = attack4Cooldown;
                break;
            case TimeKingAttackType.Attack5:
                return attack5Cooldown;
            case TimeKingAttackType.Attack6:
                return attack6Cooldown;
            case TimeKingAttackType.Attack7:
                return attack7Cooldown;
            case TimeKingAttackType.JumpAttack:
                return jumpAttackCooldown;
            default:
                cooldown = attack1Cooldown;
                break;
        }

        if (IsPhase2 &&
            (attackType == TimeKingAttackType.Attack1 ||
             attackType == TimeKingAttackType.Attack2 ||
             attackType == TimeKingAttackType.Attack3 ||
             attackType == TimeKingAttackType.Attack4))
            cooldown += phase2Attack1234ExtraCooldown;

        return cooldown;
    }

    public void BeginAttackCombo(IEnumerable<TimeKingAttackType> attacks)
    {
        attackCombo.Clear();
        foreach (TimeKingAttackType attack in attacks)
            attackCombo.Enqueue(attack);

        CurrentAttack = attackCombo.Dequeue();
        ConsumeSkillCooldown(CurrentAttack);
    }

    public bool AdvanceAttackCombo()
    {
        if (attackCombo.Count == 0)
            return false;

        CurrentAttack = attackCombo.Dequeue();
        ConsumeSkillCooldown(CurrentAttack);
        return true;
    }

    public string GetAttackAnimStateName(TimeKingAttackType attackType)
    {
        switch (attackType)
        {
            case TimeKingAttackType.Attack2:
                return "attack2";
            case TimeKingAttackType.Attack3:
                return "attack3";
            case TimeKingAttackType.Attack4:
                return "attack4";
            case TimeKingAttackType.Attack5:
                return "attack5";
            case TimeKingAttackType.Attack6:
                return "attack6";
            case TimeKingAttackType.Attack7:
                return "attack7";
            case TimeKingAttackType.JumpAttack:
                return "jump attack";
            default:
                return "attack1";
        }
    }

    public float GetAttackDuration(TimeKingAttackType attackType)
    {
        switch (attackType)
        {
            case TimeKingAttackType.Attack2:
                return attack2Duration;
            case TimeKingAttackType.Attack3:
                return attack3Duration;
            case TimeKingAttackType.Attack4:
                return attack4Duration;
            case TimeKingAttackType.Attack5:
                return attack5Duration;
            case TimeKingAttackType.Attack6:
                return attack6Duration;
            case TimeKingAttackType.Attack7:
                return attack7Duration;
            case TimeKingAttackType.JumpAttack:
                return jumpAttackAnimDuration;
            default:
                return attack1Duration;
        }
    }

    public TimeKingHitSegment[] GetMultiHitSegments(TimeKingAttackType attackType)
    {
        switch (attackType)
        {
            case TimeKingAttackType.Attack3:
                return attack3Segments;
            case TimeKingAttackType.Attack4:
                return attack4Segments;
            case TimeKingAttackType.Attack5:
                return attack5Segments;
            case TimeKingAttackType.Attack6:
                return attack6Segments;
            case TimeKingAttackType.Attack7:
                return attack7Segments;
            default:
                return attack1Segments;
        }
    }

    public bool IsMultiHitAttack(TimeKingAttackType attackType) =>
        attackType == TimeKingAttackType.Attack1 ||
        attackType == TimeKingAttackType.Attack3 ||
        attackType == TimeKingAttackType.Attack4 ||
        attackType == TimeKingAttackType.Attack5 ||
        attackType == TimeKingAttackType.Attack6 ||
        attackType == TimeKingAttackType.Attack7;

    public bool DealAttackDamage(TimeKingAttackType attackType, int segmentIndex = 0, float damageMultiplierOverride = -1f)
    {
        if (IsMultiHitAttack(attackType))
        {
            TimeKingHitSegment[] segments = GetMultiHitSegments(attackType);
            if (segments == null || segmentIndex < 0 || segmentIndex >= segments.Length)
                return false;

            TimeKingHitSegment segment = segments[segmentIndex];
            float multiplier = damageMultiplierOverride >= 0f ? damageMultiplierOverride : segment.damageMultiplier;
            return DealDamageAtHitArea(segment, multiplier);
        }

        switch (attackType)
        {
            case TimeKingAttackType.Attack2:
                return DealDamageAtHitArea(attack2Hit, 1f);
            case TimeKingAttackType.JumpAttack:
                return DealDamageAtHitArea(jumpAttackHit, 1f);
            default:
                return DealDamageAtHitArea(CreateDefaultHitArea(attackcheck, attackcheckradius), 1f);
        }
    }

    private bool DealDamageAtHitArea(TimeKingHitArea area, float damageMultiplier)
    {
        if (area == null)
            return false;

        if (area.hitCheck == null)
            area.hitCheck = attackcheck;

        if (area.hitCheck == null)
            return false;

        Collider2D[] colliders = area.GetOverlappingColliders();
        HashSet<PlayerStat> damagedTargets = new HashSet<PlayerStat>();
        bool hitAny = false;

        foreach (Collider2D hit in colliders)
        {
            PlayerStat target = hit.GetComponentInParent<PlayerStat>();
            if (target == null || !damagedTargets.Add(target))
                continue;

            Player player = hit.GetComponentInParent<Player>();
            if (player == null)
                continue;

            hitAny = true;
            AudioManager.instance.PlaySFX(1, null);

            if (target.canavoidattack(target))
            {
                Vector3 hitPos = transform.position + Vector3.up * 0.5f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPos);
                screenPos += new Vector3(Random.Range(-20f, 20f), Random.Range(0f, 20f));
                DamageNumberPool.instance.SpawnDamageNumber(screenPos, 1, false, true);
                continue;
            }

            float attackdirx = Mathf.Sign(hit.transform.position.x - transform.position.x);
            player.damage(attackdirx);

            if (!Mathf.Approximately(damageMultiplier, 1f))
                Stat.Dotimesdamage(target, damageMultiplier);
            else
                Stat.Dodamage(target);
        }

        return hitAny;
    }

    public void SyncGroundAnimator(bool walking)
    {
        string targetState = walking ? "walk" : "idle";
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(targetState))
            return;

        anim.Play(targetState, 0, 0f);
    }

    public Vector2 GetJumpLandPosition()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return transform.position;

        Transform player = playermanger.instance.player.transform;
        float landX = player.position.x + Mathf.Sign(player.position.x - transform.position.x) * jumpLandOffset;
        return new Vector2(landX, transform.position.y);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, battleDetectDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jumpAttackDistance);

        Gizmos.color = new Color(1f, 0.8f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, postAttackChaseDistance);

        if (!showAttackRangeGizmos)
            return;

        DrawSegments(attack1Segments, Color.red);
        DrawSegments(attack3Segments, Color.magenta);
        DrawSegments(attack4Segments, Color.blue);
        DrawSegments(attack5Segments, new Color(1f, 0.3f, 0.6f));
        DrawSegments(attack6Segments, new Color(0.3f, 0.8f, 1f));
        DrawSegments(attack7Segments, new Color(0.8f, 0.4f, 1f));
        DrawHitArea(attack2Hit, new Color(1f, 0.5f, 0f));
        DrawHitArea(jumpAttackHit, Color.green);
    }

    private static void DrawSegments(TimeKingHitSegment[] segments, Color color)
    {
        if (segments == null)
            return;

        foreach (TimeKingHitSegment segment in segments)
            segment?.DrawGizmo(color);
    }

    private static void DrawHitArea(TimeKingHitArea area, Color color)
    {
        area?.DrawGizmo(color);
    }

    public override bool canbestun()
    {
        if (ShouldTransform())
        {
            TryStartPhaseTransition();
            return false;
        }

        if (statemachine.currentstate == enterstate || statemachine.currentstate == changestate)
            return false;

        if (base.canbestun() && !isDead)
        {
            statemachine.changestate(stunnedstate);
            return true;
        }

        return false;
    }

    public override void Die()
    {
        BossScreenHealthBar.Hide();
        base.Die();
        statemachine.changestate(deadstate);
    }
}
