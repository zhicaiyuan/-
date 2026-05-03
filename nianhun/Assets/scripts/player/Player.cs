using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Player : Entity
{
    private Enemy enemy;
    //attack details
    public Vector2[] attackmovement;
    public float counterattackduration = .2f;
    
    public bool isbusy {  get; private set; }
    //move info
    public float movespeed = 12f;
    public float jumpforce = 12f;
    private float defaultmovespeed;
    private float defaultjumpforce;
    private float defaultdashspeed;

    //dash
    public float dashspeed;
    public float dashduration;

   
    public float dashdir {  get; private set; }


    public skillmanager skill {  get; private set; }

    //claim

  
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
       
       
        base.Awake();
 
    }


    protected override void Start()
    {
        base.Start();

        skill = skillmanager.instance;

        statemachine.initialize(idlestate);

        defaultmovespeed = movespeed;
        defaultjumpforce = jumpforce;
        defaultdashspeed = dashspeed;

    }
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


    private void checkfordash()
    {

        if(iswalldetected())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && skillmanager.instance.Dash.Canuseskill())
        {
            statemachine.changestate(dashstate);


            dashdir = Input.GetAxisRaw("Horizontal");

            if(dashdir == 0)
            {
                dashdir = facedir;
            }
        }
    }
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;

        base.Update();
        statemachine.currentstate.update(); 
        flipcontrol();
        checkfordash();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Inventory.instance.UseFlask();
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

}
