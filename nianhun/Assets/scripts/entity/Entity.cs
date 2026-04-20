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

    //knockback info
    [SerializeField] protected Vector2 knockbackdistance;
    [SerializeField] protected float knockbacktime;
    public bool isknocked;
    public bool isattack = false;
    public float attackdirx;

    //collision
    public Transform attackcheck;
    public float attackcheckradius;
    [SerializeField] protected Transform groundcheck;
    [SerializeField] protected float groundcheckdistance;
    [SerializeField] protected float wallcheckedistance;
    [SerializeField] protected Transform wallcheck;
    [SerializeField] protected LayerMask wiground;

    public int facedir { get; private set; } = 1;
    public bool faceright = true;

    public System.Action onfilped;

    protected virtual void Awake()//获取组件
    {
        fx = GetComponent<EntityFx>();
        anim = GetComponentInChildren<Animator>();
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
        StartCoroutine(hitknockback(attackdirx));
        fx.StartCoroutine("flashfx");
    }


    public virtual IEnumerator hitknockback(float attackdirx)
    {
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
        Gizmos.DrawLine(groundcheck.position, new Vector3(groundcheck.position.x, groundcheck.position.y - groundcheckdistance));
        Gizmos.DrawLine(wallcheck.position, new Vector3(wallcheck.position.x + wallcheckedistance * facedir, wallcheck.position.y));
        Gizmos.DrawWireSphere(attackcheck.position, attackcheckradius);
    }

    public virtual bool isgrounddetected()
    {
        bool isground=Physics2D.Raycast(groundcheck.position, Vector2.down, groundcheckdistance, wiground);
        if(!isground)
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
    public void flip()
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
                flip();
            else if (rb.velocity.x < 0 && faceright)
                flip();
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

    public virtual void Die()
    {

    }
}
