using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "治疗", menuName = "物品效果/特殊效果")]//类型名字
public class BuffEffect : ItemEffect
{
    private PlayerStat stat;
    [SerializeField] private StatType bufftype;
    [SerializeField] private int buffAmout;
    [SerializeField] private float buffDuration;

    public override void ExecuteEffect(Transform enemyPosition)
    {
        stat = playermanger.instance.player.GetComponent<PlayerStat>();

        stat.IncreaseStatBy(buffAmout, buffDuration, stat.GetStat(bufftype));
    }

    
}
