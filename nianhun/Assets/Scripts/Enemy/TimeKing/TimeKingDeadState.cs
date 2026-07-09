using System.Collections;
using UnityEngine;

public class TimeKingDeadState : EnemyState
{
    private TimeKing enemy;
    private SpriteRenderer sr;
    private readonly float fadespeed = .5f;

    public TimeKingDeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
        sr = enemy.GetComponentInChildren<SpriteRenderer>();
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        AudioManager.instance.PlaySFX(28, null);
        enemy.zerovelocity();
        enemy.anim.Play("die", 0, 0f);
        enemy.GetComponent<Collider2D>().enabled = false;

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        body.velocity = Vector2.zero;
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        enemy.StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(1f);

        while (sr.color.a > 0f)
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
