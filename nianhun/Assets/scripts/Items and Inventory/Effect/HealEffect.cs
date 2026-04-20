using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "治疗", menuName = "物品效果/治疗效果")]//类型名字
public class HealEffect : ItemEffect
{
    [UnityEngine.Range(0f,1f)]
    [SerializeField] private float healPercent;//治疗量
    public override void ExecuteEffect(Transform enemyPosition)
    {
        PlayerStat playerstat = playermanger.instance.player.GetComponent<PlayerStat>();

        int healAmount = Mathf.RoundToInt(playerstat.Getmaxhealthvalue() * healPercent);//计算治疗量

        playerstat.IncreaseHealthBy(healAmount);
    }//设置治疗效果
}
