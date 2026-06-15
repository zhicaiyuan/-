using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapZone : MonoBehaviour
{
    [Header("伤害")]
    [SerializeField] private int damage = 15;

    [Header("黑屏")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float blackHoldDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("安全落点")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float respawnYOffset = 0.75f;
    [SerializeField] private float groundSearchRadius = 14f;
    [SerializeField] private float groundSearchStep = 0.4f;
    [SerializeField] private Transform fallbackRespawnPoint;

    private Collider2D trapCollider;
    private bool isProcessing;

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        trapCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null || isProcessing)
            return;

        StartCoroutine(HandleTrap(player));
    }

    private IEnumerator HandleTrap(Player player)
    {
        isProcessing = true;

        PlayerStat stat = player.GetComponent<PlayerStat>();
        if (stat.isdead)
        {
            isProcessing = false;
            yield break;
        }

        player.isbusy = true;
        stat.Takedamdge(damage, false);
        stat.MakeInvincible(true);
        player.zerovelocity();

        if (stat.isdead)
        {
            isProcessing = false;
            yield break;
        }

        player.statemachine.changestate(player.trapdownstate);
        yield return player.PlayTrapKnockdownForward();

        UIFadeScreen fadeScreen = FindObjectOfType<UIFadeScreen>();
        fadeScreen?.FadeOut();
        yield return new WaitForSeconds(fadeOutDuration);

        Vector3 safePosition = FindNearestStandablePosition(player.transform.position);
        player.transform.position = safePosition;
        player.zerovelocity();

        yield return new WaitForSeconds(blackHoldDuration);

        fadeScreen?.FadeIn();
        yield return new WaitForSeconds(fadeInDuration);

        player.anim.speed = 1f;
        player.anim.SetBool("die", false);
        player.statemachine.changestate(player.idlestate);
        stat.MakeInvincible(false);
        player.isbusy = false;
        isProcessing = false;
    }

    private Vector3 FindNearestStandablePosition(Vector3 from)
    {
        if (TryFindNearestGroundPosition(from, out Vector3 groundPosition))
            return groundPosition;

        if (fallbackRespawnPoint != null)
            return fallbackRespawnPoint.position;

        return from + Vector3.up * 2f;
    }

    private bool TryFindNearestGroundPosition(Vector3 from, out Vector3 position)
    {
        position = default;
        float bestDistance = Mathf.Infinity;
        bool found = false;

        for (float offsetX = 0f; offsetX <= groundSearchRadius; offsetX += groundSearchStep)
        {
            if (TryGroundCandidate(from, offsetX, bestDistance, out Vector3 candidate, out float distance))
            {
                bestDistance = distance;
                position = candidate;
                found = true;
            }

            if (offsetX > 0f && TryGroundCandidate(from, -offsetX, bestDistance, out candidate, out distance))
            {
                bestDistance = distance;
                position = candidate;
                found = true;
            }
        }

        return found;
    }

    private bool TryGroundCandidate(Vector3 from, float offsetX, float currentBestDistance, out Vector3 candidate, out float distance)
    {
        Vector2 probeOrigin = new Vector2(from.x + offsetX, from.y + 6f);
        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down, 12f, groundLayer);
        candidate = default;
        distance = Mathf.Abs(offsetX);

        if (!hit.collider)
            return false;

        candidate = new Vector3(hit.point.x, hit.point.y + respawnYOffset, from.z);
        if (!IsOutsideTrap(candidate))
            return false;

        if (distance >= currentBestDistance)
            return false;

        return true;
    }

    private bool IsOutsideTrap(Vector3 position)
    {
        if (trapCollider == null)
            return true;

        return !trapCollider.OverlapPoint(position);
    }
}
