using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeSkillController : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>() != null)
        {
            
            Enemy enemy = collision.GetComponent<Enemy>();
            float attackdirx = -Mathf.Sign(enemy.transform.position.x - playermanger.instance.player.transform.position.x);
            enemy.damage(attackdirx);
            HitStopManager.instance.DoHitStop(.4f,1f);
            playermanger.instance.player.Stat.Dotimesdamage(enemy.Stat, 10f, true);
        }
    }
    
}
