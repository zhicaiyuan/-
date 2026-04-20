using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private int amountOfItem;//掉落数量
    [SerializeField] private ItemData[] possibleDrop;
    private List<ItemData> dropList = new List<ItemData>();//掉落物品设置

    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private ItemData item;//掉落物

    public void GenerateDrop()
    {
        for(int i = 0; i < possibleDrop.Length; i++)
        {
            if(UnityEngine.Random.Range(0,100) <= possibleDrop[i].dropchance)//如果随机值小于掉落概率
            {
                dropList.Add(possibleDrop[i]);//添加要掉落的物品
            }
        }

        for(int i = 0;i < amountOfItem; i++)
        {
            ItemData randomItem = dropList[UnityEngine.Random.Range(0,dropList.Count -1)];//随机化掉落数量

            dropList.Remove(randomItem);
            DropItem(randomItem);//掉落物品
        }

    }

    public void DropItem(ItemData itemdata)
    {
        GameObject newDrop = Instantiate(dropPrefab,transform.position, Quaternion.identity);//设置掉落组件并获取

        Vector2 randomVelocity = new Vector2(UnityEngine.Random.Range(-8,8), UnityEngine.Random.Range(15,20));//随机化掉落速度

        newDrop.GetComponent<ItemObject>().SetupItem(itemdata,randomVelocity);//弹出组件
    }
}
