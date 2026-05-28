using System.Collections;
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
    //attack info
    public float attackcheckdistance;
    public float attackcooldown;
    public float maxattackcooldown;
    public float minattackcooldown;
    [HideInInspector] public float lasttimeattack;
    public EnemyStateMachine statemachine { get; private set; }

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
    }//冰冻停止函数

    public virtual void FreezeTimeFor(float duration) => StartCoroutine(FreezeTimeCoroutine(duration));

    protected virtual IEnumerator FreezeTimeCoroutine(float seconds)
    {
        freezeTime(true);

        yield return new WaitForSeconds(seconds);

        freezeTime(false);

    }//协程用于触发暂停


    public virtual void opencounterattackwindow()
    {
        canbestunned = true;
        counterimage.SetActive(true);

    }


    public virtual void closecounterattackwindow()
    {
        canbestunned = false;
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

    protected override void OnDrawGizmos()//检测玩家的黄线
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x + attackcheckdistance * facedir,transform.position.y));
    }

    public virtual void animationfinishtrigger() => statemachine.currentstate.aniamtionfinishtrigger();//动画结束触发器

    public virtual void DealDamageToDetectedPlayers(float radiusMultiplier = 1f)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackcheck.position, attackcheckradius * radiusMultiplier);

        foreach (var hit in colliders)
        {
            Player player = hit.GetComponent<Player>();
            if (player == null)
                continue;

            AudioManager.instance.PlaySFX(1, null);
            PlayerStat target = hit.GetComponent<PlayerStat>();
            if (target.canavoidattack(target))
            {
                Vector3 hitPos = transform.position + Vector3.up * 0.5f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPos);
                screenPos += new Vector3(Random.Range(-20f, 20f), Random.Range(0f, 20f));
                DamageNumberPool.instance.SpawnDamageNumber(screenPos, 1, false, true);
                return;
            }

            float attackdirx = Mathf.Sign(hit.transform.position.x - transform.position.x);
            player.damage(attackdirx);
            Stat.Dodamage(target);
        }
    }
}
