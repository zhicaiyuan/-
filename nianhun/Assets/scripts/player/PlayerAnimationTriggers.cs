using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();
    private float[] attackradiusarry = new float[] {1f,0.8f,1.2f };//半径
    private void animationtrigger()
    {
        player.animationtrigger();
    }

    private void attacktrigger()
    {
        int currentcombo = player.anim.GetInteger("combocounter");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackcheck.position, player.attackcheckradius * attackradiusarry[currentcombo]);

        foreach(var hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                Enemystat target =hit.GetComponent<Enemystat>();
                if (target.canavoidattack(target))
                {
                    Vector3 hitPos = transform.position + Vector3.up * 0.5f;
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPos);
                    screenPos += new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(0f, 20f));
                    DamageNumberPool.instance.SpawnDamageNumber(screenPos,1,false,true);
                    return;
                }
                float attackdirx = Mathf.Sign(hit.transform.position.x - player.transform.position.x);
                enemy.damage(attackdirx);
                
                player.Stat.Dodamage(target);
                ItemDataEquipment weaponData = Inventory.instance.GetEquipment(EquipmentType.武器);//获取装备
                HitStopManager.instance.DoHitStop(.2f, .1f);
                if(weaponData != null)//如果不为空
                {
                    weaponData.Effect(target.transform);
                }
            }
                
        }//碰撞检测
    }
}
