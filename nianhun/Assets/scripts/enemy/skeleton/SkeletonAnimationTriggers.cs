using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAnimationTriggers : MonoBehaviour
{
    private Skeleton enemy => GetComponentInParent<Skeleton>();

    private void aniamtiontrigger()
    {
        enemy.animationfinishtrigger();
    }

    private void attacktrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackcheck.position, enemy.attackcheckradius);//获取攻击范围

        foreach (var hit in colliders)
        {
            Player player= hit.GetComponent<Player>();//检测到玩家
            if (player!= null)
            {
                AudioManager.instance.PlaySFX(1, null);
                PlayerStat target = hit.GetComponent<PlayerStat>();
                if (target.canavoidattack(target))
                {
                    Vector3 hitPos = transform.position + Vector3.up * 0.5f;
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPos);
                    screenPos += new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(0f, 20f));
                    DamageNumberPool.instance.SpawnDamageNumber(screenPos, 1, false, true);
                    return;
                }
                float attackdirx = Mathf.Sign(hit.transform.position.x - enemy.transform.position.x);
                player.damage(attackdirx);//判断击飞方向
                enemy.Stat.Dodamage(target);//受伤
            };
        }
    }

    private void opencounterwindow() => enemy.opencounterattackwindow();
    private void closecounterwindow() => enemy.closecounterattackwindow();//反击
}
