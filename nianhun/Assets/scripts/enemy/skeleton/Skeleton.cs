using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{
   

    #region states 
    //声明状态
    public SkeletonIdleState idlestate {  get; private set; }
    public SkeletonMoveState movestate { get; private set; }
    public SkeletonBattleState battlestate { get; private set; }
    public Skeletonattackstate attackstate { get; private set; }
    public SkeletonStunnedState stunnedstate { get; private set; }
    public SkeletonDeadState deadstate { get; private set; }
    #endregion

    protected override void Awake()
    {
        //声明状态并用状态机导入unity
        base.Awake();
        idlestate = new SkeletonIdleState(this, statemachine, "idle", this);
        movestate = new SkeletonMoveState(this,statemachine, "move", this);
        battlestate = new SkeletonBattleState(this,statemachine,"move",this);
        attackstate = new Skeletonattackstate(this, statemachine, "attack", this);
        stunnedstate = new SkeletonStunnedState(this,statemachine,"stun",this);
        deadstate = new SkeletonDeadState(this, statemachine, "die",this);
    }

    protected override void Start()
    {
        base.Start();
        if(!isDead)
            statemachine.Initialize(idlestate);//初始化状态
    }

    protected override void Update()
    {
        base.Update();
        if(isknocked && !isattack &&!isDead)
            statemachine.changestate(stunnedstate); //收到攻击转为行进状态
        
    }

    public override bool canbestun()  //判断是否可以行进
    {
        if(base.canbestun() && !isDead)
        {
            statemachine.changestate(stunnedstate);
            return true;
        }
        return false;
    }

    public override void Die() //转为死亡状态
    {
        base.Die();
        statemachine.changestate(deadstate);
    }
}
