using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour
{

    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public EntityFx fx { get; private set; }
    public CharaterStat Stat { get; private set; }
    public CapsuleCollider2D cd{ get; private set; }
    public SpriteRenderer sr { get; private set; }

    //knockback info
    [SerializeField] protected Vector2 knockbackdistance;
    [SerializeField] protected float knockbacktime;
    public bool isknocked;
    public bool isUnstoppable;
    public bool isattack = false;
    public float attackdirx;

    //collision
    public Transform attackcheck;
    public float attackcheckradius;
    [SerializeField] protected Transform groundcheck1;
    [SerializeField] protected Transform groundcheck2;
    [SerializeField] protected float groundcheckdistance;
    [SerializeField] protected float wallcheckedistance;
    [SerializeField] protected Transform wallcheck;
    [SerializeField] protected LayerMask wiground;

    [HideInInspector]public int facedir { get; private set; } = 1;
    [HideInInspector] public bool faceright = true;

    public System.Action onfilped;

    protected virtual void Awake()//获取组件
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        fx = GetComponent<EntityFx>();
        rb = GetComponent<Rigidbody2D>();
        Stat = GetComponent<CharaterStat>();
        cd = GetComponent<CapsuleCollider2D>();
    }

    protected virtual void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {

    }
    public virtual void SlowEntityBy(float slowpercentage,float slowduration)
    {

    }//减速对象

    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1;
    }//设置动画


    public virtual void damage(float attackdirx)
    {
        AudioManager.instance.PlaySFX(7, null);
        StartCoroutine(hitknockback(attackdirx));
        fx.StartCoroutine("flashfx");
    }


    public virtual IEnumerator hitknockback(float attackdirx)
    {
        if (isUnstoppable)
            yield break;
        isknocked = true;

        Vector2 knockbackvelocity = new Vector2(knockbackdistance.x * attackdirx,knockbackdistance.y);

        rb.velocity = knockbackvelocity;

        yield return new WaitForSeconds(knockbacktime);
        isknocked = false;
    }

    #region collision
    private int groundlostframe = 0;
    private int groundlostthreshold = 3;
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundcheck1.position, new Vector3(groundcheck1.position.x, groundcheck1.position.y - groundcheckdistance));
        Gizmos.DrawLine(groundcheck2.position, new Vector3(groundcheck2.position.x, groundcheck2.position.y - groundcheckdistance));
        Gizmos.DrawLine(wallcheck.position, new Vector3(wallcheck.position.x + wallcheckedistance * facedir, wallcheck.position.y));
        Gizmos.DrawWireSphere(attackcheck.position, attackcheckradius);
    }

    public virtual bool isgrounddetected()
    {
        bool isground1=Physics2D.Raycast(groundcheck1.position, Vector2.down, groundcheckdistance, wiground);
        bool isground2=Physics2D.Raycast(groundcheck2.position, Vector2.down, groundcheckdistance, wiground);
        if(!isground1 || !isground2)
        {
            groundlostframe++;
        }
        else
        {
            groundlostframe = 0;
        }
        
        return groundlostframe < groundlostthreshold;
    }

    public virtual bool iswalldetected() => Physics2D.Raycast(wallcheck.position, Vector2.right * facedir, wallcheckedistance, wiground);

    #endregion

    #region flip
    public void Flip()
    {
        facedir = facedir * -1;
        faceright = !faceright;
        transform.Rotate(0, 180, 0);

        if(onfilped != null)
            onfilped();
    }

    public void flipcontrol()
    {
        float speedthreshold = 0.1f;
        if (Mathf.Abs(rb.velocity.x) > speedthreshold)
        {
            if (rb.velocity.x > 0 && !faceright)
                Flip();
            else if (rb.velocity.x < 0 && faceright)
                Flip();
        }

    }
    
    
    #endregion

    #region velocity
    public void zerovelocity()
    {
        if (isknocked)
        {
            return;
        }
        rb.velocity = new Vector2(0, 0);
    }

    public void setvelocity(float _xvelocity, float _yvelocity)
    {
        if(isknocked)
            return;
        rb.velocity = new Vector2(_xvelocity, _yvelocity);
    }
    #endregion

    public void MakeTransprent(bool transprent)
    {
        if(transprent)
            sr.color = Color.clear;
        else
            sr.color = Color.white;
    }

    public virtual void Die()
    {

    }
}
