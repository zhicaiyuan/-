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
    private float facingLockUntil;
    public bool IsFacingLocked => isknocked || Time.time < facingLockUntil;
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
    public LayerMask GroundLayer => wiground;

    [HideInInspector]public int facedir { get; protected set; } = 1;
    [HideInInspector] public bool faceright = true;

    public System.Action onfilped;

    private MovingPlatform ridingPlatform;
    private Vector2 appliedPlatformVelocity;

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
        RefreshRidingPlatform();
    }

    protected virtual void FixedUpdate()
    {
        RefreshRidingPlatform();
    }

    protected virtual bool CanRideMovingPlatform()
    {
        return true;
    }

    public void ClearRidingPlatform()
    {
        ridingPlatform = null;
        appliedPlatformVelocity = Vector2.zero;
    }

    private Vector2 CurrentPlatformVelocity
    {
        get
        {
            if (ridingPlatform == null || !CanRideMovingPlatform())
                return Vector2.zero;
            return ridingPlatform.Velocity;
        }
    }

    private void RefreshRidingPlatform()
    {
        ridingPlatform = null;

        if (!CanRideMovingPlatform())
            return;

        ridingPlatform = FindMovingPlatformUnderFoot(groundcheck1);
        if (ridingPlatform == null)
            ridingPlatform = FindMovingPlatformUnderFoot(groundcheck2);
    }

    private MovingPlatform FindMovingPlatformUnderFoot(Transform footCheck)
    {
        if (footCheck == null)
            return null;

        float rayDistance = groundcheckdistance > 0f ? groundcheckdistance + 0.1f : 0.5f;
        Vector2 origin = (Vector2)footCheck.position + Vector2.up * 0.08f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, wiground);

        if (hit.collider == null)
            return null;

        if (!DropThroughPlatform.CountsAsGround(hit, cd))
            return null;

        return hit.collider.GetComponent<MovingPlatform>()
            ?? hit.collider.GetComponentInParent<MovingPlatform>();
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
        if (isUnstoppable)
            return;

        AudioManager.instance.PlaySFX(7, null);
        isknocked = true;
        facingLockUntil = Time.time + knockbacktime + 0.2f;
        StartCoroutine(hitknockback(attackdirx));
        fx.StartCoroutine("flashfx");
    }


    public virtual IEnumerator hitknockback(float attackdirx)
    {
        Vector2 knockbackvelocity = new Vector2(knockbackdistance.x * attackdirx, knockbackdistance.y);
        rb.velocity = knockbackvelocity;

        yield return new WaitForSeconds(knockbacktime);

        if (Mathf.Sign(rb.velocity.x) != facedir)
            rb.velocity = new Vector2(0f, rb.velocity.y);

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
        bool isground1 = IsFootOnGround(groundcheck1);
        bool isground2 = IsFootOnGround(groundcheck2);

        if (!isground1 || !isground2)
            groundlostframe++;
        else
            groundlostframe = 0;

        return groundlostframe < groundlostthreshold;
    }

    protected bool IsFootOnGround(Transform footCheck)
    {
        if (footCheck == null)
            return false;

        float rayDistance = groundcheckdistance > 0f ? groundcheckdistance + 0.1f : 0.5f;
        Vector2 origin = (Vector2)footCheck.position + Vector2.up * 0.08f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, wiground);

        if (hit.collider == null)
            return false;

        return DropThroughPlatform.CountsAsGround(hit, cd);
    }

    public virtual bool iswalldetected() => Physics2D.Raycast(wallcheck.position, Vector2.right * facedir, wallcheckedistance, wiground);

    public virtual bool iswalldetected(int direction)
    {
        if (direction == 0)
            return false;

        return Physics2D.Raycast(wallcheck.position, Vector2.right * direction, wallcheckedistance, wiground);
    }

    public virtual bool canMoveInDirection(int direction)
    {
        if (!isgrounddetected())
            return false;

        if (direction == 0)
            return true;

        return !iswalldetected(direction);
    }

    #endregion

    #region flip
    public void LockFacing(float duration)
    {
        facingLockUntil = Mathf.Max(facingLockUntil, Time.time + duration);
    }

    public virtual void Flip()
    {
        if (IsFacingLocked)
            return;

        facedir = facedir * -1;
        faceright = !faceright;
        transform.Rotate(0, 180, 0);

        if(onfilped != null)
            onfilped();
    }

    public void flipcontrol()
    {
        if (IsFacingLocked)
            return;

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

        Vector2 platform = CurrentPlatformVelocity;
        rb.velocity = platform;
        appliedPlatformVelocity = platform;
    }

    public void setvelocity(float _xvelocity, float _yvelocity)
    {
        if(isknocked)
            return;

        Vector2 platform = CurrentPlatformVelocity;
        // 调用方常传入 rb.velocity.y，需先去掉上一次叠上去的平台速度，避免重复叠加
        float baseY = _yvelocity - appliedPlatformVelocity.y;
        rb.velocity = new Vector2(_xvelocity + platform.x, baseY + platform.y);
        appliedPlatformVelocity = platform;
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
