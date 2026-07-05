using System.Collections;
using UnityEngine;

public class WalkingStickDeadState : EnemyState
{
    private WalkingStick enemy;
    private SpriteRenderer sr;
    private float fadespeed = .5f;

    public WalkingStickDeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, WalkingStick walkingStick)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = walkingStick;
        sr = enemy.GetComponentInChildren<SpriteRenderer>();
    }

    public override void enter()
    {
        base.enter();
        AudioManager.instance.PlaySFX(12, null);
        enemy.anim.SetBool("move", false);
        enemy.anim.SetBool("idle", false);
        enemy.anim.SetBool("stun", false);
        enemy.anim.SetBool("attack", false);
        enemy.anim.SetBool("die", true);
        enemy.GetComponent<Collider2D>().enabled = false;

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        body.velocity = Vector2.zero;
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        enemy.StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(1);

        while (sr.color.a > 0)
        {
            Color c = sr.color;
            c.a -= fadespeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }

        Object.Destroy(enemy.gameObject);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();
    }
}
