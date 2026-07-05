using UnityEngine;

public class Mushroom : Enemy
{
    public const float CombatDetectDistance = 2f;

    #region states
    public MushroomIdleState idlestate { get; private set; }
    public MushroomMoveState movestate { get; private set; }
    public MushroomBattleState battlestate { get; private set; }
    public MushroomAttackState attackstate { get; private set; }
    public MushroomStunnedState stunnedstate { get; private set; }
    public MushroomDeadState deadstate { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        idlestate = new MushroomIdleState(this, statemachine, "idle", this);
        movestate = new MushroomMoveState(this, statemachine, "move", this);
        battlestate = new MushroomBattleState(this, statemachine, "move", this);
        attackstate = new MushroomAttackState(this, statemachine, "attack", this);
        stunnedstate = new MushroomStunnedState(this, statemachine, "stun", this);
        deadstate = new MushroomDeadState(this, statemachine, "die", this);
    }

    protected override void Start()
    {
        base.Start();
        if (!isDead)
            statemachine.Initialize(idlestate);
    }

    protected override void Update()
    {
        base.Update();
        if (isknocked && !isattack && !isDead)
            statemachine.changestate(stunnedstate);
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
