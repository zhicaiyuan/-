using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class RootBoss : Enemy
{
    public RootBossIdleState idlestate { get; private set; }
    public RootBossMoveState movestate { get; private set; }
    public RootBossDeadState deadstate { get; private set; }
    public RootBossChangeState changetate { get; private set; }
    public RootBossbattleState battlestate { get; private set; }
    public RootBossAttack1State attack1state { get; private set; }
    public RootBossStunnedState stunnedstate { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        idlestate = new RootBossIdleState(this, statemachine, "Idle");
        movestate = new RootBossMoveState(this, statemachine, "Move");
        deadstate = new RootBossDeadState(this, statemachine, "Die");
        changetate = new RootBossChangeState(this, statemachine, "Change");
        battlestate = new RootBossbattleState(this, statemachine, "Move");
        attack1state = new RootBossAttack1State(this, statemachine, "Attack");
        stunnedstate = new RootBossStunnedState(this, statemachine, "Stun");
    }
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
