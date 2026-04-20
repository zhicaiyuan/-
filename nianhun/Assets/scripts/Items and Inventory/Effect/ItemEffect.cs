using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "新物品信息", menuName = "物品效果")]//类型名字
public class ItemEffect : ScriptableObject
{
    public virtual void ExecuteEffect(Transform enemyPosition)
    {
        Debug.Log("施加效果");
    }
}
