using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkeletonDeadState : EnemyState
{
    private Skeleton enemy;
    private SpriteRenderer sr;
    private float fadespeed= .5f;
public SkeletonDeadState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
        sr = enemy.GetComponentInChildren<SpriteRenderer>();
    }

    public override void aniamtionfinishtrigger()
    {
        base.aniamtionfinishtrigger();
    }


    public override void enter()
    {

        base.enter();
        AudioManager.instance.PlaySFX(12, null);
        enemy.anim.SetBool("move",false);
        enemy.anim.SetBool("idle", false);
        enemy.anim.SetBool("stun", false);
        enemy.anim.SetBool("attack", false);
        enemy.anim.SetBool("die", true);
        enemy.GetComponent<Collider2D>().enabled = false;//取消碰撞防止鞭尸
        Rigidbody2D rb =enemy.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;//冻结敌人位置
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        enemy.StartCoroutine(fasdeanddestory());
    }
        
    IEnumerator fasdeanddestory()
    {
        yield return new WaitForSeconds(3);
        while (sr.color.a > 0)
        {
            Color c = sr.color;
            c.a -= fadespeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }
        UnityEngine.Object.Destroy(enemy.gameObject);

    }


    public override void update()
    {
        base.update();
        
        enemy.zerovelocity();
        
    }
}
