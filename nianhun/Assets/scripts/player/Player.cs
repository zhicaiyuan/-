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
    
    public bool isbusy {  get;  set; }
    [Header("移动信息")]
    public float movespeed = 12f;
    public float jumpforce = 12f;
    public int jumpchance;
    private bool wasGrounded;
    [HideInInspector] public int dashchance;
    [HideInInspector] public bool jumpKeyDown;
    private bool alreadyjumped;
    private float defaultmovespeed;
    private float defaultjumpforce;
    private float defaultdashspeed;

    //dash
    public float dashspeed;
    public float dashduration;

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
    public PlayerBlackHoleState blackholestate { get; private set; }
    public PlayerSpinState spinstate { get; private set; }
    public PlayerStrikeSkillState strikeSkillState { get; private set; }
    public PlayerLaserState laserState { get; private set; }
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

        blackholestate = new PlayerBlackHoleState(this, statemachine, "jump");
        spinstate = new PlayerSpinState(this, statemachine, "Spin");
        strikeSkillState = new PlayerStrikeSkillState(this, statemachine, "Strike");
        laserState = new PlayerLaserState(this, statemachine, "Laser");
        base.Awake();
 
    }


    protected override void Start()
    {
        base.Start();

        skill = SkillManager.instance;

        statemachine.initialize(idlestate);

        defaultmovespeed = movespeed;
        defaultjumpforce = jumpforce;
        defaultdashspeed = dashspeed;

    }
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;
        base.Update();
        alreadyjumped = false;
        jumpKeyDown = Input.GetKeyDown(KeyCode.K);

        SetChance();
        SetDrag();
        statemachine.currentstate.Update();
        flipcontrol();
        CheckForDash();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Inventory.instance.UseFlask();
        }
    }

    private void SetDrag()
    {
        if (isgrounddetected())
            rb.drag = 3f;
        else
            rb.drag = 0f;
    }//设置阻力

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
        dashspeed = dashspeed * (1 - slowpercentage);
        anim.speed =anim.speed * (1 - slowpercentage);

        Invoke("ReturnDefaultSpeed", slowduration);
    }//设置减速

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        movespeed = defaultmovespeed;
        jumpforce = defaultjumpforce;
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


    public override void Die()
    {
        
        base.Die(); 
        statemachine.changestate(deadstate);
    }

    public bool TryJump()
    {
        if ((alreadyjumped))
                return false;
        if(jumpchance <= 0)
            return false;
        
            statemachine.changestate(jumpstate);
            jumpchance--;
            alreadyjumped = true;
            return true;
        
    }


}
