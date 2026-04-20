using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "冰冻敌人", menuName = "物品效果/冰冻效果")]//类型名字
public class FreezeEffect : ItemEffect
{
    [SerializeField] private float duartion;

    public override void ExecuteEffect(Transform transform)
    {
        PlayerStat playerstat = playermanger.instance.player.GetComponent<PlayerStat>();

        if(playerstat.currenthealth > playerstat.Getmaxhealthvalue() * .1f)
            return;

        if(!Inventory.instance.CanUseArmor())
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5);//设置圆形碰撞箱

        foreach(var hit in colliders)//对于每次碰撞
        {
            hit.GetComponent<Enemy>()?.FreezeTimeFor(duartion); 
        }
    }
}
