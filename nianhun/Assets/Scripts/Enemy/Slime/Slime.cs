using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeType
{
    big,
    medium,
    small
}

public class Slime : Enemy
{
    [Header("Slime spesific")]
    [SerializeField] private SlimeType slimeType;
    [SerializeField] private int slimesToCreate;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private Vector2 mincreationVelocity;
    [SerializeField] private Vector2 maxcreationVelocity;
    [SerializeField] private float splitSpawnAttackLockDuration = 1f;

    private float attackLockedUntil;
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

        if (slimeType == SlimeType.small)
            return;

        CreateSlimes(slimesToCreate, slimePrefab);
    }
    private void CreateSlimes(int amountofSlimes,GameObject slimePrefab) //生成小史莱姆
    {
        for (int i = 0; i < amountofSlimes; i++)
        {
            GameObject newslime = Instantiate(slimePrefab, transform.position, Quaternion.identity);
            
            newslime.GetComponent<Slime>().SetupSlime(facedir);
        }
    }

    public void SetupSlime(int facediring)
    {
        if(facediring != facedir)
            Flip();

        float xVelocity = Random.Range(mincreationVelocity.x, maxcreationVelocity.x);
        float yVelocity = Random.Range(mincreationVelocity.y, maxcreationVelocity.y);

        isknocked = true;
        LockAttackFor(splitSpawnAttackLockDuration);

        GetComponent<Rigidbody2D>().velocity = new Vector2(xVelocity * facedir, yVelocity);

        Invoke("CancelKnockback", 1.5f);
    }

    public bool CanAttack() => Time.time >= attackLockedUntil;

    private void LockAttackFor(float duration)
    {
        attackLockedUntil = Time.time + duration;
        lasttimeattack = Time.time;
    }

    private void CancelKnockback() => isknocked = false;
}