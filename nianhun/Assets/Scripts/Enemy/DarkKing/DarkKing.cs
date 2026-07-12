using System.Collections.Generic;
using UnityEngine;

public class DarkKing : Enemy
{
    public DarkKingIdleState idlestate { get; private set; }
    public DarkKingBattleState battlestate { get; private set; }
    public DarkKingAttackState attackstate { get; private set; }
    public DarkKingTeleportState teleportstate { get; private set; }
    public DarkKingSummonHandsState summonhandsstate { get; private set; }
    public DarkKingHandRainState handrainstate { get; private set; }
    public DarkKingRecoveryState recoverystate { get; private set; }
    public DarkKingStunnedState stunnedstate { get; private set; }
    public DarkKingDeadState deadstate { get; private set; }

    private readonly Dictionary<DarkKingAttackType, float> lastSkillUsedTime = new Dictionary<DarkKingAttackType, float>();
    private readonly HashSet<int> appliedHitKeys = new HashSet<int>();

    public DarkKingAttackType CurrentAttack { get; set; }
    public bool HasEnteredBattle { get; private set; }
    public bool IsPhase2 { get; private set; }
    public bool IsPhase3 { get; private set; }

    private SpriteRenderer[] cachedSpriteRenderers;
    private Collider2D[] cachedColliders;
    private bool[] cachedSpriteEnabled;
    private bool[] cachedColliderEnabled;
    private bool combatPhysicsSuspended;
    private float suspendedSafeY;
    private float suspendedGravityScale = 4f;

    [Header("暗黑王 战斗")]
    public float battleDetectDistance = 12f;
    public float battleLoseDistance = 20f;
    public float recoveryDuration = 1.2f;
    public float minGapBetweenAttacks = 0.15f;

    [Header("普攻")]
    [SerializeField] private DarkKingHitArea attackHit;
    public float attackDuration = 1.2f;
    public float attackHitTime = 0.5f;
    public float attackCooldown = 0.2f;
    public float attackDamageMultiplier = 1f;
    [Tooltip("弹反窗口相对出伤时间提前开启")]
    public float attackCounterOpenBeforeHit = 0.25f;
    [Tooltip("出伤后弹反窗口再保持多久")]
    public float attackCounterCloseAfterHit = 0.1f;

    [Header("二阶段 传送")]
    public float teleportOutDuration = 0.4f;
    public float teleportWarningDuration = 0.7f;
    public float teleportInDuration = 0.8f;
    public float teleportHitTime = 0.25f;
    public float teleportBehindOffset = 1.5f;
    public float teleportCooldown = 8f;
    public float teleportDamageMultiplier = 1.5f;
    public float teleportTriggerDistance = 3f;
    public Vector2 teleportWarningSize = new Vector2(2.2f, 2.2f);
    [SerializeField] private DarkKingHitArea teleportHit;

    [Header("三阶段 鬼手召唤")]
    public int summonHandsCount = 3;
    public float summonHandsXRange = 2.5f;
    public float summonHandsMaxDistance = 10f;
    public float summonHandsStartDelay = 0.25f;
    [Tooltip("红框追踪玩家的时间，结束后才出爪")]
    public float summonHandsWarningDuration = 1.6f;
    [Tooltip("红框追随速度，应低于玩家移速以便躲避")]
    public float summonHandsFollowSpeed = 3.2f;
    [Tooltip("红色加深锁定后，再过多久出爪")]
    public float summonHandsCommitDelay = 0.3f;
    public float summonHandsStagger = 0.4f;
    public float summonHandsDuration = 3.8f;
    public float summonHandsCooldown = 10f;
    public float summonHandsHitRadius = 1.2f;
    public float summonHandsDamageMultiplier = 1f;
    public Vector2 summonHandsWarningSize = new Vector2(1.6f, 1.6f);

    [Header("三阶段 鬼手雨")]
    public int handRainCount = 5;
    public float handRainInterval = 0.55f;
    public float handRainWarningDuration = 0.55f;
    [Tooltip("红色加深锁定后，再过多久出爪")]
    public float handRainCommitDelay = 0.5f;
    public float handRainXJitter = 1.2f;
    public float handRainOutDuration = 0.35f;
    public float handRainAppearDuration = 0.7f;
    public float handRainCooldown = 14f;
    public float handRainHitRadius = 1.2f;
    public float handRainDamageMultiplier = 1f;
    public Vector2 handRainWarningSize = new Vector2(1.6f, 1.6f);

    [Header("预警 / FX")]
    public Color warningColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    [SerializeField] private GameObject clawFxPrefab;
    public float clawFxYOffset = 0.8f;
    public float clawFxLifeTime = 0.8f;

    private float lastRecoveryEndTime = -999f;
    private float lastFlipTime;
    private const float FaceDeadzone = 0.2f;
    private const float FlipCooldown = 0.25f;
    private Transform worldHealthBarRoot;
    private Transform visualRoot;
    [Tooltip("相对 animator 的翻转锚点（身体中心）。默认 0 表示钉住 animator 位置，避免包围盒跳动闪现。")]
    [SerializeField] private Vector3 flipAnchorLocalOffset = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        EnsureDefaultHits();
        CacheCombatVisuals();
        visualRoot = anim != null ? anim.transform : transform.Find("animator");

        worldHealthBarRoot = transform.Find("entity-stat-UI");
        if (worldHealthBarRoot != null)
            worldHealthBarRoot.gameObject.SetActive(false);

        idlestate = new DarkKingIdleState(this, statemachine, "DarkKingIdle", this);
        battlestate = new DarkKingBattleState(this, statemachine, "DarkKingBattle", this);
        attackstate = new DarkKingAttackState(this, statemachine, "DarkKingAttack", this);
        teleportstate = new DarkKingTeleportState(this, statemachine, "DarkKingTeleport", this);
        summonhandsstate = new DarkKingSummonHandsState(this, statemachine, "DarkKingSummonHands", this);
        handrainstate = new DarkKingHandRainState(this, statemachine, "DarkKingHandRain", this);
        recoverystate = new DarkKingRecoveryState(this, statemachine, "DarkKingRecovery", this);
        stunnedstate = new DarkKingStunnedState(this, statemachine, "DarkKingStunned", this);
        deadstate = new DarkKingDeadState(this, statemachine, "DarkKingDead", this);
    }

    protected override void Start()
    {
        base.Start();
        Stat.onhealthchanged += OnHealthChangedForPhase;
        if (!isDead)
            statemachine.Initialize(idlestate);
    }

    private void OnDestroy()
    {
        if (Stat != null)
            Stat.onhealthchanged -= OnHealthChangedForPhase;
    }

    protected override void Update()
    {
        base.Update();
        EnsureVulnerableOutsideProtectedStates();

        if (isknocked && !isattack && !isDead
            && statemachine.currentstate != teleportstate
            && statemachine.currentstate != handrainstate)
            statemachine.changestate(stunnedstate);
    }

    private void EnsureVulnerableOutsideProtectedStates()
    {
        if (isDead
            || statemachine.currentstate == teleportstate
            || statemachine.currentstate == handrainstate)
            return;

        if (Stat.isInvincible)
            Stat.MakeInvincible(false);
    }

    private void OnHealthChangedForPhase()
    {
        if (isDead || Stat.currenthealth <= 0)
            return;

        if (!IsPhase2 && !IsAboveTwoThirdsHealth())
            IsPhase2 = true;

        if (IsPhase2 && !IsPhase3 && !IsAboveOneThirdHealth())
            IsPhase3 = true;
    }

    public bool IsAboveTwoThirdsHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() * 2f / 3f;
    public bool IsAboveOneThirdHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() / 3f;

    public void MarkEnteredBattle()
    {
        HasEnteredBattle = true;
        if (worldHealthBarRoot != null)
            worldHealthBarRoot.gameObject.SetActive(false);
        BossScreenHealthBar.Show(Stat);
    }

    public void MarkRecoveryStarted() => lastRecoveryEndTime = Time.time + recoveryDuration;

    public bool IsComboReady() => Time.time >= lastRecoveryEndTime + minGapBetweenAttacks;

    public bool IsSkillReady(DarkKingAttackType attackType)
    {
        if (!IsComboReady())
            return false;

        return Time.time >= GetLastSkillUsedTime(attackType) + GetSkillCooldown(attackType);
    }

    public void ConsumeSkillCooldown(DarkKingAttackType attackType) =>
        lastSkillUsedTime[attackType] = Time.time;

    private float GetLastSkillUsedTime(DarkKingAttackType attackType) =>
        lastSkillUsedTime.TryGetValue(attackType, out float time) ? time : -999f;

    public float GetSkillCooldown(DarkKingAttackType attackType)
    {
        switch (attackType)
        {
            case DarkKingAttackType.Teleport:
                return teleportCooldown;
            case DarkKingAttackType.SummonHands:
                return summonHandsCooldown;
            case DarkKingAttackType.HandRain:
                return handRainCooldown;
            default:
                return attackCooldown;
        }
    }

    public void EnterSkill(DarkKingAttackType attackType)
    {
        CurrentAttack = attackType;
        ConsumeSkillCooldown(attackType);

        switch (attackType)
        {
            case DarkKingAttackType.Teleport:
                statemachine.changestate(teleportstate);
                break;
            case DarkKingAttackType.SummonHands:
                statemachine.changestate(summonhandsstate);
                break;
            case DarkKingAttackType.HandRain:
                statemachine.changestate(handrainstate);
                break;
            default:
                statemachine.changestate(attackstate);
                break;
        }
    }

    public void ResetAttackHitTracking() => appliedHitKeys.Clear();

    public bool TryDealAttackDamage()
    {
        int key = ((int)DarkKingAttackType.Attack << 8);
        if (!appliedHitKeys.Add(key))
            return false;

        return DealDamageAtHitArea(attackHit, attackDamageMultiplier);
    }

    public bool TryDealTeleportDamage()
    {
        int key = ((int)DarkKingAttackType.Teleport << 8);
        if (!appliedHitKeys.Add(key))
            return false;

        return DealDamageAtHitArea(teleportHit, teleportDamageMultiplier);
    }

    public bool DealGhostHandDamage(Vector2 worldPosition, int index, float damageMultiplier, float radius)
    {
        int key = (1000 << 8) | index;
        if (!appliedHitKeys.Add(key))
            return false;

        return DealDamageAtWorldPoint(worldPosition, radius, damageMultiplier);
    }

    private bool DealDamageAtHitArea(DarkKingHitArea area, float damageMultiplier)
    {
        if (area == null)
            return false;

        if (area.hitCheck == null)
            area.hitCheck = attackcheck;

        if (area.hitCheck == null)
            return false;

        return ApplyDamageToColliders(area.GetOverlappingColliders(), damageMultiplier);
    }

    public bool DealDamageAtWorldPoint(Vector2 worldCenter, float radius, float damageMultiplier)
    {
        return ApplyDamageToColliders(Physics2D.OverlapCircleAll(worldCenter, radius), damageMultiplier);
    }

    private bool ApplyDamageToColliders(Collider2D[] colliders, float damageMultiplier)
    {
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

    public void SpawnClawFx(Vector2 worldPosition)
    {
        Vector3 pos = new Vector3(worldPosition.x, worldPosition.y + clawFxYOffset, 0f);

        if (clawFxPrefab != null)
        {
            GameObject fx = Instantiate(clawFxPrefab, pos, Quaternion.identity);
            Destroy(fx, clawFxLifeTime + 0.1f);
            return;
        }

        // 无 prefab 时用红色半透明方块占位，避免完全没反馈
        DarkKingAttackWarning.Show(pos, new Vector2(1.2f, 1.8f), clawFxLifeTime, new Color(0.8f, 0.1f, 0.1f, 0.7f));
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

    /// <summary>
    /// 根节点 Y180 翻转（碰撞体一起转）。
    /// 用稳定的 animator 锚点做 2*offset 预补偿，避免 sprite.bounds 每帧跳动造成闪现。
    /// </summary>
    public override void Flip()
    {
        if (IsFacingLocked)
            return;

        if (visualRoot == null)
            visualRoot = anim != null ? anim.transform : transform.Find("animator");

        float offsetX = 0f;
        if (visualRoot != null)
        {
            Vector3 anchorWorld = visualRoot.TransformPoint(flipAnchorLocalOffset);
            offsetX = anchorWorld.x - transform.position.x;
        }

        facedir = facedir * -1;
        faceright = !faceright;
        transform.Rotate(0, 180, 0);

        // Y180 会把锚点镜像到另一侧；根节点平移 2*offset 钉住世界 X
        if (!Mathf.Approximately(offsetX, 0f))
        {
            Vector3 pos = transform.position;
            pos.x += 2f * offsetX;
            transform.position = pos;
            if (rb != null)
                rb.position = new Vector2(pos.x, rb.position.y);
        }

        if (onfilped != null)
            onfilped();
    }

    public Vector2 GetTeleportLandPosition()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return transform.position;

        Player player = playermanger.instance.player;
        float behind = -player.facedir;
        if (Mathf.Approximately(behind, 0f))
            behind = faceright ? -1f : 1f;

        float landX = player.transform.position.x + behind * teleportBehindOffset;
        return new Vector2(landX, GetCombatGroundY());
    }

    public Vector2 GetGhostHandPositionNearPlayerX(float xJitter)
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return new Vector2(transform.position.x, GetCombatGroundY());

        Transform player = playermanger.instance.player.transform;
        float x = player.position.x + Random.Range(-xJitter, xJitter);
        return new Vector2(x, GetCombatGroundY());
    }

    /// <summary>隐身穿地前记录的地面高度；未隐身则用当前 Y。</summary>
    public float GetCombatGroundY() =>
        combatPhysicsSuspended ? suspendedSafeY : transform.position.y;

    public void SyncGroundAnimator(bool walking)
    {
        string targetState = walking ? "walk" : "idle";
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(targetState))
            return;

        anim.Play(targetState, 0, 0f);
    }

    public void CacheCombatVisuals()
    {
        cachedSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        cachedColliders = GetComponentsInChildren<Collider2D>(true);
        cachedSpriteEnabled = new bool[cachedSpriteRenderers.Length];
        cachedColliderEnabled = new bool[cachedColliders.Length];

        for (int i = 0; i < cachedSpriteRenderers.Length; i++)
            cachedSpriteEnabled[i] = cachedSpriteRenderers[i].enabled;

        for (int i = 0; i < cachedColliders.Length; i++)
            cachedColliderEnabled[i] = cachedColliders[i].enabled;
    }

    public void SetCombatVisible(bool visible)
    {
        if (cachedSpriteRenderers == null || cachedColliders == null)
            CacheCombatVisuals();

        for (int i = 0; i < cachedSpriteRenderers.Length; i++)
        {
            if (cachedSpriteRenderers[i] == null)
                continue;

            if (visible)
                cachedSpriteRenderers[i].enabled = cachedSpriteEnabled[i];
            else
            {
                cachedSpriteEnabled[i] = cachedSpriteRenderers[i].enabled;
                cachedSpriteRenderers[i].enabled = false;
            }
        }

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] == null)
                continue;

            if (visible)
                cachedColliders[i].enabled = cachedColliderEnabled[i];
            else
            {
                cachedColliderEnabled[i] = cachedColliders[i].enabled;
                cachedColliders[i].enabled = false;
            }
        }

        // 关掉碰撞后若仍受重力会穿地，现身卡在地里；隐身时冻结重力并钉住安全高度
        if (!visible)
            SuspendCombatPhysics();
        else
            ResumeCombatPhysics();
    }

    private void SuspendCombatPhysics()
    {
        if (combatPhysicsSuspended)
            return;

        combatPhysicsSuspended = true;
        suspendedSafeY = transform.position.y;

        if (rb == null)
            return;

        suspendedGravityScale = rb.gravityScale;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.position = new Vector2(transform.position.x, suspendedSafeY);
    }

    private void ResumeCombatPhysics()
    {
        if (!combatPhysicsSuspended)
            return;

        combatPhysicsSuspended = false;

        Vector3 pos = transform.position;
        pos.y = suspendedSafeY;
        transform.position = pos;

        if (rb == null)
            return;

        rb.gravityScale = suspendedGravityScale;
        rb.velocity = Vector2.zero;
        rb.position = new Vector2(pos.x, pos.y);
    }

    private void EnsureDefaultHits()
    {
        if (attackHit == null || attackHit.hitCheck == null)
            attackHit = CreateDefaultHit(attackcheck, 1.2f);

        if (teleportHit == null || teleportHit.hitCheck == null)
            teleportHit = CreateDefaultHit(attackcheck, 1.4f);
    }

    private static DarkKingHitArea CreateDefaultHit(Transform check, float radius)
    {
        return new DarkKingHitArea
        {
            hitCheck = check,
            shape = DarkKingHitShape.Circle,
            radius = radius,
            boxSize = new Vector2(radius * 2f, radius * 2f)
        };
    }

    public override void damage(float attackdirx)
    {
        if (isDead || Stat.isInvincible)
            return;

        AudioManager.instance.PlaySFX(7, null);
        fx.StartCoroutine("flashfx");
    }

    public override bool canbestun()
    {
        if (statemachine.currentstate == teleportstate ||
            statemachine.currentstate == handrainstate)
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
        SetCombatVisible(true);
        BossScreenHealthBar.Hide();
        base.Die();
        statemachine.changestate(deadstate);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, battleDetectDistance);
        attackHit?.DrawGizmo(Color.red);
        teleportHit?.DrawGizmo(Color.magenta);
    }
}
