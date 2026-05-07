using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SpinSkillController : MonoBehaviour
{
    private Coroutine damageCoroutine;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            if (damageCoroutine != null)
            {
            StopCoroutine(damageCoroutine);
            }
            damageCoroutine = StartCoroutine(WaitAndDamage(collision));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>() != null && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }
    IEnumerator WaitAndDamage(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        while (enemy != null)
        {
            float attackdirx = -Mathf.Sign(enemy.transform.position.x - playermanger.instance.player.transform.position.x);
            enemy.damage(attackdirx);
            playermanger.instance.player.Stat.Dotimesdamage(enemy.Stat,.15f);
            yield return new WaitForSeconds(1f);
        }
    }
}
