using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeSkillController : MonoBehaviour
{
    private readonly HashSet<int> hitEnemyIds = new HashSet<int>();
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
        StartCoroutine(ScanHits());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private IEnumerator ScanHits()
    {
        // 判定框可能在动画中途才启用；连续扫几帧，避免 Boss 已在范围内却收不到 Enter
        for (int i = 0; i < 8; i++)
        {
            RefreshOverlapsAndHit();
            yield return null;
        }
    }

    private void RefreshOverlapsAndHit()
    {
        if (hitCollider == null || !hitCollider.enabled)
            return;

        overlapBuffer.Clear();
        Physics2D.OverlapCollider(hitCollider, contactFilter, overlapBuffer);

        for (int i = 0; i < overlapBuffer.Count; i++)
            TryHit(overlapBuffer[i]);
    }

    private void TryHit(Collider2D collision)
    {
        Enemy enemy = collision != null ? collision.GetComponentInParent<Enemy>() : null;
        if (enemy == null || enemy.isDead)
            return;

        int id = enemy.GetInstanceID();
        if (!hitEnemyIds.Add(id))
            return;

        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        float attackdirx = -Mathf.Sign(enemy.transform.position.x - playermanger.instance.player.transform.position.x);
        enemy.damage(attackdirx);

        if (HitStopManager.instance != null)
            HitStopManager.instance.DoHitStop(.4f, 1f);

        playermanger.instance.player.Stat.Dotimesdamage(enemy.Stat, 3f, true);
    }
}
