using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneSkillController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;
    private Player player;

    [SerializeField] private float colorlosingSpeed;
    private float cloneTimer;
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = .8f;
    private Transform closestEnemy;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = playermanger.instance.player;
    }
    private void Update()
    {
        cloneTimer -= Time.deltaTime;
        if( cloneTimer < 0)
        {
            sr.color = new Color(1, 1, 1, sr.color.a - (Time.deltaTime * colorlosingSpeed));

            if (sr.color.a <= 0)
                Destroy(gameObject);
        }
    }
    public void SetUpClone(Transform newtransform,float cloneDuration,bool canAttack,Vector3 offset)
    {
        if (canAttack)
            anim.SetInteger("attacknumber", Random.Range(1, 3));

        transform.position = newtransform.position + offset;
        cloneTimer = cloneDuration;

        FaceClosestTarget();
    }
    private void animationtrigger()
    {
        cloneTimer = -.1f;
    }

    private void attacktrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);

        foreach (var hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                Enemystat target = hit.GetComponent<Enemystat>();
                if (target.canavoidattack(target))
                {
                    Vector3 hitPos = transform.position + Vector3.up * 0.5f;
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPos);
                    screenPos += new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(0f, 20f));
                    DamageNumberPool.instance.SpawnDamageNumber(screenPos, 1, false, true);
                    return;
                }
                float attackdirx = Mathf.Sign(hit.transform.position.x - player.transform.position.x);
                enemy.damage(attackdirx);

                player.Stat.Dodamage(target);
                ItemDataEquipment weaponData = Inventory.instance.GetEquipment(EquipmentType.武器);//获取装备
                HitStopManager.instance.DoHitStop(.2f, .1f);
                if (weaponData != null)//如果不为空
                {
                    weaponData.Effect(target.transform);
                }
            }

        }//碰撞检测
    }

    private void FaceClosestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 25);

        float closestdistance = Mathf.Infinity;
        foreach(var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);

                if (distanceToEnemy < closestdistance)
                    closestEnemy = hit.transform;
            }
        }

        if(closestEnemy != null)
        {
            if (transform.position.x > closestEnemy.position.x)
                transform.Rotate(0, 180, 0);
        }
    }
}
