using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Player : Entity
{
    private Enemy enemy;
    #region
    [Header("攻击设置")]
    public Vector2[] attackmovement;
    public float counterattackduration = .2f;
    public float couterattackcooldawn;
    public float couterattacktimer;

    [Header("攻击撞墙回弹")]
    [SerializeField] private float attackWallRecoilX = 10f;
    [SerializeField] private float attackWallRecoilY = 3f;
    [SerializeField] private float attackWallRecoilFacingLock = 0.18f;
    [SerializeField] private float attackWallBounceIFrames = 0.25f;
    private Coroutine attackWallBounceProtectionRoutine;

    public bool isbusy { get; set; }
    public bool skipNextPostAttackBusy { get; set; }

    [Header("移动信息")]
    public float movespeed = 12f;
    public float jumpforce = 12f;
    [SerializeField] private float minJumpForce = 8f;
    [SerializeField] private float maxJumpHoldTime = 0.45f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float attackBufferTime = 0.2f;
    public int jumpchance;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private bool canCoyoteJump = true;
    private float attackBufferCounter;
    [HideInInspector] public int dashchance;
    [HideInInspector] public bool jumpKeyDown;
    private bool alreadyjumped;
    private float defaultmovespeed;
    private float defaultjumpforce;
    private float defaultminjumpforce;
    private float defaultdashspeed;
    private bool isJumpBoostActive;
    private float jumpHoldTimer;
    private float pendingJumpForce = -1f;

    //dash
    public float dashspeed;
    public float dashduration;

    [Header("祈祷存档")]
    public float prayDuration = 1.5f;
    public Checkpoint NearbyCheckpoint { get; private set; }

    [Header("下跳平台")]
    [SerializeField] private float dropThroughDuration = 0.35f;
    [SerializeField] private float dropThroughDownForce = 5f;
    public bool IsDroppingThroughPlatform { get; private set; }

    private Coroutine dropThroughRoutine;

    [HideInInspector] public BlackHoleSkill blackHole;
    [HideInInspector] public SpinSkill spin;
    [HideInInspector] public StrikeSkill strike;
    [HideInInspector]public LaserSkill laser;

    #endregion
    public float dashdir {  get; private set; }


    public SkillManager skill {  get; private set; }

    #region


    public PlayerStateMachine statemachine { get; private set; }
    
    public PlayerIdleState idlestate { get; private set; }
    public PlayerMoveState movestate { get; private set; }
    public PlayerJumpState jumpstate { get; private set; }

    public PlayerDashState dashstate { get; private set; }

    public PlayerWallslideState wallslide { get; private set; }

    public PlayerAirState airstate { get; private set; }

    public PlayerWallJump playerwalljump { get; private set; }
    public PlayerPrimaryAttack primaryattack { get; private set; }
    public CounterAttackState counterattackstate { get; private set; }

    public PlayerDeadState deadstate { get; private set; }
    public PlayerTrapDownState trapdownstate { get; private set; }
    public PlayerBlackHoleState blackholestate { get; private set; }
    public PlayerSpinState spinstate { get; private set; }
    public PlayerStrikeSkillState strikeSkillState { get; private set; }
    public PlayerLaserState laserState { get; private set; }
    public PlayerPrayState prayState { get; private set; }
    public PlayerAutoWalkState autowalkstate { get; private set; }
    #endregion
    //状态声明


    protected override void Awake()
    {
        statemachine = new PlayerStateMachine();

        idlestate = new PlayerIdleState(this,statemachine,"idle");
        movestate = new PlayerMoveState(this, statemachine, "move");
        jumpstate = new PlayerJumpState(this, statemachine, "jump");
        airstate = new PlayerAirState(this,statemachine,"air");
        dashstate= new PlayerDashState(this,statemachine,"dash");
        wallslide = new PlayerWallslideState(this, statemachine, "wallslide");
        playerwalljump = new PlayerWallJump(this, statemachine, "jump");

       primaryattack = new PlayerPrimaryAttack(this,statemachine,"attack");
        counterattackstate = new CounterAttackState(this, statemachine, "counterattack");
        deadstate = new PlayerDeadState(this, statemachine, "die");
        trapdownstate = new PlayerTrapDownState(this, statemachine, "die");

        blackholestate = new PlayerBlackHoleState(this, statemachine, "jump");
        spinstate = new PlayerSpinState(this, statemachine, "Spin");
        strikeSkillState = new PlayerStrikeSkillState(this, statemachine, "Strike");
        laserState = new PlayerLaserState(this, statemachine, "Laser");
        prayState = new PlayerPrayState(this, statemachine, "Pray");
        autowalkstate = new PlayerAutoWalkState(this, statemachine, "move");
        base.Awake();
 
    }


    protected override void Start()
    {
        base.Start();

        skill = SkillManager.instance;

        statemachine.initialize(idlestate);

        defaultmovespeed = movespeed;
        defaultjumpforce = jumpforce;
        defaultminjumpforce = minJumpForce;
        defaultdashspeed = dashspeed;

    }
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;
        base.Update();
        alreadyjumped = false;
        jumpKeyDown = Input.GetKeyDown(KeyCode.K);
        UpdateAttackBuffer();

        SetCoyoteTime();
        SetChance();
        SetDrag();
        statemachine.currentstate.Update();
        TryDropThroughPlatform();
        flipcontrol();
        CheckForDash();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Inventory.instance.UseFlask();
        }
    }

    protected override bool CanRideMovingPlatform()
    {
        return !IsDroppingThroughPlatform;
    }

    private void SetDrag()
    {
        if (IsDroppingThroughPlatform)
            rb.drag = 0f;
        else if (isgrounddetected())
            rb.drag = 3f;
        else
            rb.drag = 0f;
    }//设置阻力

    private void SetCoyoteTime()
    {
        if (isgrounddetected())
        {
            coyoteTimeCounter = coyoteTime;
            canCoyoteJump = true;
            return;
        }

        coyoteTimeCounter -= Time.deltaTime;
    }

    public bool CanCoyoteJump()
    {
        return canCoyoteJump
            && coyoteTimeCounter > 0f
            && !iswalldetected()
            && !IsDroppingThroughPlatform;
    }

    private void UpdateAttackBuffer()
    {
        if (Input.GetKeyDown(KeyCode.J))
            attackBufferCounter = attackBufferTime;
        else if (attackBufferCounter > 0f)
            attackBufferCounter -= Time.deltaTime;
    }

    public bool TryConsumeAttackInput()
    {
        if (attackBufferCounter <= 0f)
            return false;

        attackBufferCounter = 0f;
        return true;
    }

    public bool HasAttackBuffer() => attackBufferCounter > 0f;

    public void ClearAttackBuffer() => attackBufferCounter = 0f;

    public bool TryBeginParryCancelFromAttack()
    {
        if (skill == null || skill.parry == null || !skill.parry.parryUnlocked)
            return false;

        skipNextPostAttackBusy = true;
        ClearAttackBuffer();
        return true;
    }

    public bool ConsumeSkipPostAttackBusy()
    {
        if (!skipNextPostAttackBusy)
            return false;

        skipNextPostAttackBusy = false;
        return true;
    }

    public bool IsAttackBlockedByWall()
    {
        if (iswalldetected())
            return true;

        if (attackcheck == null)
            return false;

        return Physics2D.Raycast(
            attackcheck.position,
            Vector2.right * facedir,
            attackcheckradius + 0.15f,
            GroundLayer);
    }

    public void ApplyAttackWallRecoil()
    {
        LockFacing(attackWallRecoilFacingLock);
        setvelocity(-facedir * attackWallRecoilX, attackWallRecoilY);
    }

    public void BeginAttackWallBounce()
    {
        if (attackWallBounceProtectionRoutine != null)
            StopCoroutine(attackWallBounceProtectionRoutine);

        Stat.MakeInvincible(true);
        isUnstoppable = true;
        ApplyAttackWallRecoil();
        attackWallBounceProtectionRoutine = StartCoroutine(AttackWallBounceProtectionRoutine());
    }

    private IEnumerator AttackWallBounceProtectionRoutine()
    {
        float duration = Mathf.Max(attackWallBounceIFrames, attackWallRecoilFacingLock);
        yield return new WaitForSeconds(duration);

        Stat.MakeInvincible(false);
        isUnstoppable = false;
        attackWallBounceProtectionRoutine = null;
    }

    private void SetChance()
    {
        bool grounded = isgrounddetected();
        if (grounded && !wasGrounded)
        {
            if(SkillManager.instance.doubleJump.doubleJumpUnlocked)
                jumpchance = 1;
            else
                jumpchance = 0;

            dashchance = 1;
        }
        wasGrounded = grounded;
    }//重置跳跃冲刺次数

    public override void SlowEntityBy(float slowpercentage, float slowduration)
    {
        movespeed = movespeed * (1 - slowpercentage);
        jumpforce = jumpforce * (1 - slowpercentage);
        minJumpForce = minJumpForce * (1 - slowpercentage);
        dashspeed = dashspeed * (1 - slowpercentage);
        anim.speed =anim.speed * (1 - slowpercentage);

        Invoke("ReturnDefaultSpeed", slowduration);
    }//设置减速

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        movespeed = defaultmovespeed;
        jumpforce = defaultjumpforce;
        minJumpForce = defaultminjumpforce;
        dashspeed = defaultdashspeed;
    }


    private void CheckForDash()
    {
        if (isbusy)
            return;

        if (skill.dash.dashUnlocked == false)
            return;

        if(iswalldetected())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && SkillManager.instance.dash.Canuseskill() && dashchance > 0)
        {
            statemachine.changestate(dashstate);


            dashdir = Input.GetAxisRaw("Horizontal");

            if(dashdir == 0)
            {
                dashdir = facedir;
            }
        }
    }

    public IEnumerator busyfor(float _seconds)
    {
        isbusy = true;

        yield return new WaitForSeconds(_seconds);

        isbusy = false; 
    }

    public void animationtrigger() => statemachine.currentstate.animationfinishtrigger();

    private static readonly int TrapDieStateHash = Animator.StringToHash("playerdie");

    public IEnumerator PlayTrapKnockdownForward()
    {
        anim.SetBool("die", true);
        anim.speed = 1f;

        yield return WaitForDeathAnimation();
    }

    public IEnumerator WaitForDeathAnimation()
    {
        yield return null;
        yield return WaitForAnimatorState(TrapDieStateHash);

        while (IsPlayingTrapDieAnimation() && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.98f)
            yield return null;
    }

    private IEnumerator WaitForAnimatorState(int stateHash)
    {
        while (anim.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
            yield return null;
    }

    private bool IsPlayingTrapDieAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).shortNameHash == TrapDieStateHash;
    }


    public override void Die()
    {
        
        base.Die(); 
        statemachine.changestate(deadstate);
    }

    public void SetNearbyCheckpoint(Checkpoint checkpoint) => NearbyCheckpoint = checkpoint;

    public void ClearNearbyCheckpoint(Checkpoint checkpoint)
    {
        if (NearbyCheckpoint == checkpoint)
            NearbyCheckpoint = null;
    }

    public override bool isgrounddetected()
    {
        if (IsDroppingThroughPlatform)
            return false;

        return base.isgrounddetected();
    }

    private void TryDropThroughPlatform()
    {
        if (isbusy || IsDroppingThroughPlatform || dropThroughRoutine != null)
            return;

        if (!IsDropInputPressed())
            return;

        if (!TryGetPlatformBelow(out Collider2D platformCollider, out float platformSurfaceY))
            return;

        dropThroughRoutine = StartCoroutine(DropThroughPlatformRoutine(platformCollider, platformSurfaceY));
    }

    private bool IsDropInputPressed()
    {
        // 方向键下留给视角移动；Vertical 轴也包含方向键，故不用于下平台
        return Input.GetKeyDown(KeyCode.S)
            || Input.GetKeyDown(KeyCode.Keypad2)
            || Input.GetKeyDown(KeyCode.Keypad5);
    }

    private bool TryGetPlatformBelow(out Collider2D platformCollider, out float platformSurfaceY)
    {
        platformCollider = null;
        platformSurfaceY = 0f;

        if (rb.velocity.y > 2f)
            return false;

        float rayDistance = groundcheckdistance > 0f ? groundcheckdistance + 0.2f : 0.4f;
        float bestDistance = float.MaxValue;

        foreach (Vector2 origin in GetFootCheckOrigins())
        {
            Vector2 rayOrigin = origin + Vector2.up * 0.05f;
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayDistance, wiground);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == cd)
                    continue;

                if (!DropThroughPlatform.IsDropThroughCollider(hit.collider))
                    continue;

                if (hit.distance >= bestDistance)
                    continue;

                bestDistance = hit.distance;
                platformCollider = hit.collider;
                platformSurfaceY = hit.collider.bounds.max.y;
            }
        }

        return platformCollider != null;
    }

    private Vector2[] GetFootCheckOrigins()
    {
        if (groundcheck1 != null && groundcheck2 != null)
            return new[] { (Vector2)groundcheck1.position, (Vector2)groundcheck2.position };

        return new[]
        {
            new Vector2(cd.bounds.min.x, cd.bounds.min.y),
            new Vector2(cd.bounds.max.x, cd.bounds.min.y)
        };
    }

    private IEnumerator DropThroughPlatformRoutine(Collider2D platformCollider, float platformSurfaceY)
    {
        IsDroppingThroughPlatform = true;
        canCoyoteJump = false;
        coyoteTimeCounter = 0f;
        Physics2D.IgnoreCollision(cd, platformCollider, true);

        statemachine.changestate(airstate);
        rb.velocity = new Vector2(rb.velocity.x, -Mathf.Abs(dropThroughDownForce));

        yield return new WaitForFixedUpdate();

        float timeout = Time.time + 1.5f;
        while (cd.bounds.min.y >= platformSurfaceY - 0.05f && Time.time < timeout)
            yield return new WaitForFixedUpdate();

        yield return new WaitForSeconds(dropThroughDuration);

        Physics2D.IgnoreCollision(cd, platformCollider, false);
        IsDroppingThroughPlatform = false;
        dropThroughRoutine = null;
    }

    public bool TryJump()
    {
        if ((alreadyjumped))
                return false;
        if(jumpchance <= 0)
            return false;
        
            pendingJumpForce = jumpforce;
            isJumpBoostActive = false;
            statemachine.changestate(jumpstate);
            jumpchance--;
            alreadyjumped = true;
            return true;
        
    }

    public void ExecuteGroundJump()
    {
        canCoyoteJump = false;
        pendingJumpForce = minJumpForce;
        jumpHoldTimer = 0f;
        isJumpBoostActive = true;
        statemachine.changestate(jumpstate);
    }

    public float ConsumeJumpForce()
    {
        float force = pendingJumpForce >= 0f ? pendingJumpForce : jumpforce;
        pendingJumpForce = -1f;
        return force;
    }

    public void UpdateJumpBoost(float deltaTime)
    {
        if (!isJumpBoostActive || !Input.GetKey(KeyCode.K) || rb.velocity.y <= 0f)
            return;

        jumpHoldTimer += deltaTime;
        float holdRatio = Mathf.Clamp01(jumpHoldTimer / maxJumpHoldTime);
        float targetVy = Mathf.Lerp(minJumpForce, jumpforce, holdRatio);

        if (rb.velocity.y < targetVy)
            rb.velocity = new Vector2(rb.velocity.x, targetVy);

        if (jumpHoldTimer >= maxJumpHoldTime)
            isJumpBoostActive = false;
    }

    public void ApplyJumpCut()
    {
        isJumpBoostActive = false;

        if (rb.velocity.y <= 0f)
            return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
    }


}
