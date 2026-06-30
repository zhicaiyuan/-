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
    [SerializeField] private float respawnYOffset = 0.05f;
    [SerializeField] private float groundSearchRadius = 22f;
    [SerializeField] private float groundSearchStep = 0.35f;
    [SerializeField] private float probeHeight = 8f;
    [SerializeField] private float maxRayDistance = 30f;
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
            player.isbusy = false;
            isProcessing = false;
            yield break;
        }

        player.statemachine.changestate(player.trapdownstate);
        yield return player.PlayTrapKnockdownForward();

        UIFadeScreen fadeScreen = FindObjectOfType<UIFadeScreen>();
        if (fadeScreen != null)
            yield return fadeScreen.FadeOutRoutine(fadeOutDuration);
        else
            yield return new WaitForSeconds(fadeOutDuration);

        Vector3 safePosition = FindNearestStandablePosition(player);
        player.transform.position = safePosition;
        player.zerovelocity();

        yield return new WaitForSeconds(blackHoldDuration);

        if (fadeScreen != null)
            yield return fadeScreen.FadeInRoutine(fadeInDuration);
        else
            yield return new WaitForSeconds(fadeInDuration);

        player.anim.speed = 1f;
        player.anim.SetBool("die", false);
        player.statemachine.changestate(player.idlestate);
        stat.MakeInvincible(false);
        player.isbusy = false;
        isProcessing = false;
    }

    private Vector3 FindNearestStandablePosition(Player player)
    {
        Vector3 searchOrigin = GetRespawnSearchOrigin(player);
        NearestPlatformFinder.Settings settings = BuildSearchSettings(player);

        if (NearestPlatformFinder.TryFind(searchOrigin, in settings, out Vector3 platformPosition))
            return platformPosition;

        if (fallbackRespawnPoint != null)
            return fallbackRespawnPoint.position;

        return searchOrigin + Vector3.up * 2f;
    }

    private Vector3 GetRespawnSearchOrigin(Player player)
    {
        if (fallbackRespawnPoint != null)
            return fallbackRespawnPoint.position;

        if (trapCollider != null)
        {
            Bounds bounds = trapCollider.bounds;
            float x = Mathf.Clamp(player.transform.position.x, bounds.min.x, bounds.max.x);
            return new Vector3(x, bounds.max.y, player.transform.position.z);
        }

        return player.transform.position;
    }

    private NearestPlatformFinder.Settings BuildSearchSettings(Player player)
    {
        LayerMask layer = groundLayer.value != 0 ? groundLayer : player.GroundLayer;
        bool pitStyleTrap = IsPitStyleTrap();

        return new NearestPlatformFinder.Settings
        {
            groundLayer = layer,
            bodyCollider = player.cd,
            excludeColliders = pitStyleTrap ? GetOtherTrapColliders(this) : GetAllTrapColliders(),
            partialTrapVolume = pitStyleTrap ? trapCollider : null,
            standGap = respawnYOffset,
            searchRadius = groundSearchRadius,
            horizontalStep = groundSearchStep,
            probeHeight = probeHeight,
            maxRayDistance = maxRayDistance,
            verticalSearchBoost = pitStyleTrap ? 2f : 6f,
            upwardPenaltyWeight = pitStyleTrap ? 2.5f : 0.35f,
            maxUpwardFromOrigin = pitStyleTrap ? 1.5f : -1f
        };
    }

    private bool IsPitStyleTrap()
    {
        if (trapCollider == null)
            return false;

        Vector2 size = trapCollider.bounds.size;
        return size.y > 3f && size.y > size.x * 1.25f;
    }

    public static Collider2D[] GetOtherTrapColliders(TrapZone exclude)
    {
        TrapZone[] trapZones = Object.FindObjectsOfType<TrapZone>();
        if (trapZones == null || trapZones.Length == 0)
            return null;

        int count = 0;
        for (int i = 0; i < trapZones.Length; i++)
        {
            if (trapZones[i] != exclude)
                count++;
        }

        if (count == 0)
            return null;

        Collider2D[] colliders = new Collider2D[count];
        int index = 0;
        for (int i = 0; i < trapZones.Length; i++)
        {
            if (trapZones[i] == exclude)
                continue;

            colliders[index++] = trapZones[i].TrapCollider;
        }

        return colliders;
    }

    public static Collider2D[] GetAllTrapColliders()
    {
        TrapZone[] trapZones = Object.FindObjectsOfType<TrapZone>();
        if (trapZones == null || trapZones.Length == 0)
            return null;

        Collider2D[] colliders = new Collider2D[trapZones.Length];
        for (int i = 0; i < trapZones.Length; i++)
            colliders[i] = trapZones[i].TrapCollider;

        return colliders;
    }

    public Collider2D TrapCollider => trapCollider;
}
