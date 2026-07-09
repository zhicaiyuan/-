using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSkillController : MonoBehaviour
{
    private Coroutine damageCoroutine;
    [SerializeField] private float interval = 1f;
    private readonly HashSet<Enemy> targets = new HashSet<Enemy>();
    private readonly List<Enemy> damageBuffer = new List<Enemy>();
    private Collider2D hitCollider;
    private readonly ContactFilter2D contactFilter = new ContactFilter2D
    {
        useTriggers = true,
        useLayerMask = false
    };
    private readonly List<Collider2D> overlapBuffer = new List<Collider2D>(16);

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
            hitCollider = GetComponentInChildren<Collider2D>();
    }

    private void OnEnable()
    {
        RefreshOverlappingTargets();
        EnsureDamageLoop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryGetEnemy(other, out Enemy enemy))
        {
            targets.Add(enemy);
            EnsureDamageLoop();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (TryGetEnemy(other, out Enemy enemy))
            targets.Remove(enemy);
    }

    private void EnsureDamageLoop()
    {
        if (damageCoroutine == null && targets.Count > 0)
            damageCoroutine = StartCoroutine(DamageLoop());
    }

    private IEnumerator DamageLoop()
    {
        while (true)
        {
            RefreshOverlappingTargets();

            if (targets.Count == 0)
            {
                damageCoroutine = null;
                yield break;
            }

            damageBuffer.Clear();
            damageBuffer.AddRange(targets);

            for (int i = 0; i < damageBuffer.Count; i++)
            {
                Enemy enemy = damageBuffer[i];
                if (enemy != null && !enemy.isDead)
                    ApplyDamage(enemy);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void RefreshOverlappingTargets()
    {
        if (hitCollider == null || !hitCollider.enabled)
            return;

        overlapBuffer.Clear();
        Physics2D.OverlapCollider(hitCollider, contactFilter, overlapBuffer);

        for (int i = 0; i < overlapBuffer.Count; i++)
        {
            if (TryGetEnemy(overlapBuffer[i], out Enemy enemy) && !enemy.isDead)
                targets.Add(enemy);
        }

        targets.RemoveWhere(enemy => enemy == null || enemy.isDead);
    }

    private static bool TryGetEnemy(Collider2D other, out Enemy enemy)
    {
        enemy = other != null ? other.GetComponentInParent<Enemy>() : null;
        return enemy != null;
    }

    private void ApplyDamage(Enemy enemy)
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        float attackdirx = Mathf.Sign(enemy.transform.position.x - playermanger.instance.player.transform.position.x);
        enemy.damage(attackdirx);
        playermanger.instance.player.Stat.Dotimesdamage(enemy.Stat, 0.8f);
    }
}
