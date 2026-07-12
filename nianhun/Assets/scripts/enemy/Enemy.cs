using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Enemystat))]
[RequireComponent(typeof(EntityFx))]
[RequireComponent(typeof(ItemDrop))]
public class Enemy : Entity
{
    public bool isDead = false;
    [SerializeField] protected LayerMask whatisplayer;
    //stunned info
    public float stuntime;
    public Vector2 stundirection;
    protected bool canbestunned;
    [SerializeField] protected GameObject counterimage;

    //move info
    public float movespeed;
    private float defaultmovespeed;
    public float idletime;
    public float battletime;
    [SerializeField] private float walkTime = 5f;
    private bool flipWhenIdleEnter;
    //attack info
    public float attackcheckdistance;
    public float attackcooldown;
    public float maxattackcooldown;
    public float minattackcooldown;
    [HideInInspector] public float lasttimeattack;
    public EnemyStateMachine statemachine { get; private set; }

    private int lastAttackHitFrame = -1;

    public string lastAnimboolname {  get; private set; }

    protected override void Awake()
    {
        base.Awake();
        statemachine = new EnemyStateMachine();
        defaultmovespeed = movespeed;
    }

    protected override void Update()
    {
        base.Update();
        statemachine.currentstate.update();
        
        
    }

    public virtual void freezeTime(bool timeFrozen)
    {
        if (timeFrozen)
        {
            movespeed = 0;
            anim.speed = 0;
        }
        else
        {
            movespeed = defaultmovespeed;
            anim.speed = 1;
        }
    }//����ֹͣ����

    public virtual void FreezeTimeFor(float duration) => StartCoroutine(FreezeTimeCoroutine(duration));

    protected virtual IEnumerator FreezeTimeCoroutine(float seconds)
    {
        freezeTime(true);

        yield return new WaitForSeconds(seconds);

        freezeTime(false);

    }//Э�����ڴ�����ͣ


    public virtual void opencounterattackwindow()
    {
        canbestunned = true;
        if (counterimage != null)
            counterimage.SetActive(true);
    }


    public virtual void closecounterattackwindow()
    {
        canbestunned = false;
        if (counterimage != null)
            counterimage.SetActive(false);
    }

    public override void SlowEntityBy(float slowpercentage, float slowduration)
    {
       movespeed = movespeed * (1 -  slowpercentage);
        anim.speed = anim.speed * (1 - slowpercentage);
    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        movespeed = defaultmovespeed;
    }

    public virtual void AssignlastAnimName(string animboolname)
    {
        lastAnimboolname = animboolname;
    }

    public virtual bool canbestun()
    {
        if(canbestunned)
        {
            closecounterattackwindow();
            return true;
        }
        return false;
    }

    public virtual RaycastHit2D ispalyerdetected() => Physics2D.Raycast(wallcheck.position,Vector2.right * facedir,20,whatisplayer);

    public float WalkTime => walkTime;

    public void QueuePatrolTurnAround() => flipWhenIdleEnter = true;

    public void ApplyPatrolTurnAroundOnIdleEnter()
    {
        if (!flipWhenIdleEnter)
            return;

        flipWhenIdleEnter = false;
        Flip();
    }

    protected override void OnDrawGizmos()//�����ҵĻ���
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackcheckdistance * facedir,transform.position.y));
    }

    public virtual void animationfinishtrigger() => statemachine.currentstate.aniamtionfinishtrigger();//��������������

    public virtual bool DealDamageToDetectedPlayers(
        float radiusMultiplier = 1f,
        int fixedDamageOverride = -1,
        float damageMultiplier = 1f,
        bool useSharedHitFrameGuard = true,
        Vector2? worldCenterOverride = null)
    {
        if (useSharedHitFrameGuard && lastAttackHitFrame == Time.frameCount)
            return false;

        Vector2 center = worldCenterOverride ?? (Vector2)attackcheck.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, attackcheckradius * radiusMultiplier);
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

            if (useSharedHitFrameGuard)
                lastAttackHitFrame = Time.frameCount;

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

            if (fixedDamageOverride >= 0)
                Stat.DoFixedDamage(target, fixedDamageOverride);
            else if (!Mathf.Approximately(damageMultiplier, 1f))
                Stat.Dotimesdamage(target, damageMultiplier);
            else
                Stat.Dodamage(target);
        }

        return hitAny;
    }
}
