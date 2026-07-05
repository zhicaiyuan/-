using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SpinSkillController : MonoBehaviour
{
    private Coroutine damageCoroutine;
    [SerializeField] private float interval = .8f;
    private HashSet<GameObject> targets = new HashSet<GameObject>();

    public void SetAttackInterval(float seconds)
    {
        interval = Mathf.Max(0.01f, seconds);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() != null)
        {
            targets.Add(other.gameObject); // 添加目标到集合
            if (damageCoroutine == null)
                damageCoroutine = StartCoroutine(DamageLoop());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() != null)
        {
            targets.Remove(other.gameObject); // 从集合中移除目标
            if (targets.Count == 0 && damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator DamageLoop()
    {
        while (targets.Count > 0)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    ApplyDamage(target);
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private void ApplyDamage(GameObject target)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            float attackdirx = Mathf.Sign(enemy.transform.position.x - playermanger.instance.player.transform.position.x);
            enemy.damage(attackdirx);
            playermanger.instance.player.Stat.Dotimesdamage(enemy.Stat, 0.5f);
        }
    }
}
