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
                float attackdirx = Mathf.Sign(hit.transform.position.x - enemy.transform.position.x);
                player.damage(attackdirx);//判断击飞方向
                PlayerStat target = hit.GetComponent<PlayerStat>();
                enemy.Stat.Dodamage(target);//受伤

                
            };
        }
    }

    private void opencounterwindow() => enemy.opencounterattackwindow();
    private void closecounterwindow() => enemy.closecounterattackwindow();//反击
}
