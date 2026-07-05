using UnityEngine;

public class WalkingStick : Enemy
{
    public WalkingStickIdleState idlestate { get; private set; }
    public WalkingStickWalkState walkstate { get; private set; }
    public WalkingStickAttackState attackstate { get; private set; }
    public WalkingStickStunnedState stunnedstate { get; private set; }
    public WalkingStickDeadState deadstate { get; private set; }

    [Header("WalkingStick 战斗")]
    public float detectDistance = 8f;
    [Tooltip("二阶段 Attack3 触发距离，应小于等于 attackcheckdistance")]
    public float attack3TriggerDistance = 1f;

    [Header("攻击判定")]
    [Tooltip("Attack1：发现玩家时的起身刺击，判定点最近、范围最小")]
    [SerializeField] private Transform attackCheck1;
    [SerializeField] private float attackCheck1Radius = 0.9f;
    [Tooltip("Attack2：一阶段默认攻击，判定点稍远、范围中等")]
    [SerializeField] private Transform attackCheck2;
    [SerializeField] private float attackCheck2Radius = 1.1f;
    [Tooltip("Attack3：二阶段追击近身大招，判定点最前、范围最大")]
    [SerializeField] private Transform attackCheck3;
    [SerializeField] private float attackCheck3Radius = 1.3f;

    [Header("攻击节奏")]
    public float attack1Duration = 1.1f;
    public float attack1HitTime = 0.45f;
    public float attack2Duration = 1f;
    public float attack2HitTime = 0.4f;
    public float attack3Duration = 1.15f;
    public float attack3HitTime = 0.5f;

    public bool HasUsedAttack1 { get; private set; }
    public bool IsPhase2 { get; private set; }
    public WalkingStickAttackType CurrentAttack { get; set; }

    protected override void Awake()
    {
        base.Awake();
        idlestate = new WalkingStickIdleState(this, statemachine, "idle", this);
        walkstate = new WalkingStickWalkState(this, statemachine, "move", this);
        attackstate = new WalkingStickAttackState(this, statemachine, "attack", this);
        stunnedstate = new WalkingStickStunnedState(this, statemachine, "stun", this);
        deadstate = new WalkingStickDeadState(this, statemachine, "die", this);
    }

    protected override void Start()
    {
        base.Start();
        SetSuperArmor(true);

        if (!isDead)
            statemachine.Initialize(idlestate);

        Stat.onhealthchanged += OnHealthChangedForPhaseTransition;
    }

    private void OnDestroy()
    {
        if (Stat != null)
            Stat.onhealthchanged -= OnHealthChangedForPhaseTransition;
    }

    protected override void Update()
    {
        base.Update();
        if (isknocked && !isattack && !isDead && !isUnstoppable)
            statemachine.changestate(stunnedstate);
    }

    private void OnHealthChangedForPhaseTransition()
    {
        if (IsPhase2 || isDead || IsAboveHalfHealth())
            return;

        EnterPhase2();
    }

    public bool IsAboveHalfHealth() => Stat.currenthealth > Stat.Getmaxhealthvalue() / 2;

    public void EnterPhase2()
    {
        if (IsPhase2 || isDead)
            return;

        IsPhase2 = true;
        lasttimeattack = Time.time;

        if (statemachine.currentstate == attackstate)
            return;

        statemachine.changestate(walkstate);
    }

    public void MarkAttack1Used() => HasUsedAttack1 = true;

    public void SetSuperArmor(bool enabled) => isUnstoppable = enabled;

    public bool IsPlayerDetected()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        Transform player = playermanger.instance.player.transform;
        if (Vector2.Distance(transform.position, player.position) <= detectDistance)
            return true;

        RaycastHit2D hit = ispalyerdetected();
        return hit.collider != null;
    }

    public bool IsPlayerInAttackRange()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        return Vector2.Distance(transform.position, playermanger.instance.player.transform.position) <= attackcheckdistance;
    }

    public bool IsPlayerInAttack3Range()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        float triggerDistance = attack3TriggerDistance > 0f ? attack3TriggerDistance : attackcheckdistance;
        return Vector2.Distance(transform.position, playermanger.instance.player.transform.position) <= triggerDistance;
    }

    public void FacePlayer()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        float dx = playermanger.instance.player.transform.position.x - transform.position.x;
        if (dx > 0.05f && !faceright)
            Flip();
        else if (dx < -0.05f && faceright)
            Flip();
    }

    public bool IsAttackReady() => Time.time >= lasttimeattack + attackcooldown;

    public void ConsumeAttackCooldown()
    {
        attackcooldown = Random.Range(minattackcooldown, maxattackcooldown);
        lasttimeattack = Time.time;
    }

    public float GetAttackDuration(WalkingStickAttackType attackType)
    {
        switch (attackType)
        {
            case WalkingStickAttackType.Attack2:
                return attack2Duration;
            case WalkingStickAttackType.Attack3:
                return attack3Duration;
            default:
                return attack1Duration;
        }
    }

    public float GetAttackHitTime(WalkingStickAttackType attackType)
    {
        switch (attackType)
        {
            case WalkingStickAttackType.Attack2:
                return attack2HitTime;
            case WalkingStickAttackType.Attack3:
                return attack3HitTime;
            default:
                return attack1HitTime;
        }
    }

    public string GetAttackAnimStateName(WalkingStickAttackType attackType)
    {
        switch (attackType)
        {
            case WalkingStickAttackType.Attack2:
                return "attck2";
            case WalkingStickAttackType.Attack3:
                return "attck3";
            default:
                return "attack1";
        }
    }

    public bool DealAttackDamage(WalkingStickAttackType attackType, float damageMultiplier = 1f)
    {
        GetAttackCheck(attackType, out Transform check, out float radius);
        if (check == null)
            check = attackcheck;

        if (radius <= 0f)
            radius = attackcheckradius;

        float radiusMultiplier = attackcheckradius > 0f ? radius / attackcheckradius : 1f;
        return DealDamageToDetectedPlayers(
            radiusMultiplier,
            damageMultiplier: damageMultiplier,
            worldCenterOverride: check.position);
    }

    public void SyncGroundAnimator(bool walking)
    {
        anim.SetBool("attack", false);
        anim.SetBool("stun", false);
        anim.SetBool("die", false);
        anim.SetBool("idle", !walking);
        anim.SetBool("move", walking);
    }

    private void GetAttackCheck(WalkingStickAttackType attackType, out Transform check, out float radius)
    {
        switch (attackType)
        {
            case WalkingStickAttackType.Attack2:
                check = attackCheck2 != null ? attackCheck2 : attackcheck;
                radius = attackCheck2Radius;
                break;
            case WalkingStickAttackType.Attack3:
                check = attackCheck3 != null ? attackCheck3 : attackcheck;
                radius = attackCheck3Radius;
                break;
            default:
                check = attackCheck1 != null ? attackCheck1 : attackcheck;
                radius = attackCheck1Radius;
                break;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        DrawAttackGizmo(attackCheck1, attackCheck1Radius, Color.red);
        DrawAttackGizmo(attackCheck2, attackCheck2Radius, Color.yellow);
        DrawAttackGizmo(attackCheck3, attackCheck3Radius, Color.magenta);
    }

    private void DrawAttackGizmo(Transform check, float radius, Color color)
    {
        if (check == null)
            return;

        Gizmos.color = color;
        Gizmos.DrawWireSphere(check.position, radius);
    }

    public override bool canbestun()
    {
        if (isUnstoppable)
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
        base.Die();
        statemachine.changestate(deadstate);
    }
}
