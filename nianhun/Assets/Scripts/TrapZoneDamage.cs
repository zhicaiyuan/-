using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapZoneDamage : MonoBehaviour
{
    [Header("伤害")]
    [SerializeField] private int damage = 15;

    

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



        player.statemachine.changestate(player.idlestate);
        stat.MakeInvincible(false);
        player.isbusy = false;
        isProcessing = false;
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
