using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    #region States
    public SlimeIdleState idlestate { get; private set; }
    public SlimeMoveState movestate { get; private set; }
    public SlimeDeadState deadstate { get; private set; }
    public SlimeAttackState attackstate { get; private set; }
    public SlimeBattleState battlestate { get; private set; }
    public SlimeStunnedState stunnedstate { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        idlestate = new SlimeIdleState(this, statemachine, "Idle", this);
        movestate = new SlimeMoveState(this, statemachine, "Move", this);
        deadstate = new SlimeDeadState(this, statemachine, "Die", this);
        attackstate = new SlimeAttackState(this, statemachine, "Attack", this);
        battlestate = new SlimeBattleState(this, statemachine, "Move", this);
        stunnedstate = new SlimeStunnedState(this, statemachine, "Stun", this);
    }
    #endregion

    protected override void Start()
    {
        base.Start();

        statemachine.Initialize(idlestate);
    }

    public override bool canbestun()  //判断是否可以行进
    {
        if (base.canbestun() && !isDead)
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