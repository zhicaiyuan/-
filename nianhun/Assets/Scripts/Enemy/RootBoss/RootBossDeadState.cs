using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootBossDeadState : EnemyState
{
    private RootBoss enemy;
    private SpriteRenderer sr;
    private float fadespeed = .5f;


    public RootBossDeadState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname,RootBoss enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
        this.sr = enemy.GetComponentInChildren<SpriteRenderer>();
    }
        
    public override void aniamtionfinishtrigger()
    {
        base.aniamtionfinishtrigger();
    }


    public override void enter()
    {

        base.enter();
        AudioManager.instance.PlaySFX(28, null);
        enemy.anim.SetBool("Move", false);
        enemy.anim.SetBool("Idle", false);
        enemy.anim.SetBool("Stun", false);
        enemy.anim.SetBool("Attack", false);
        enemy.anim.SetBool("Change", false);
        enemy.anim.SetBool("Dash", false);
        enemy.anim.SetBool("Die", true);
        enemy.GetComponent<Collider2D>().enabled = false;//取消碰撞防止鞭尸
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;//冻结敌人位置
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        enemy.StartCoroutine(fasdeanddestory());
    }

    IEnumerator fasdeanddestory()
    {
        yield return new WaitForSeconds(1);
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
