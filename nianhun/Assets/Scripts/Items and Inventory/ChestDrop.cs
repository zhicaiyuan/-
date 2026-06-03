using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : MonoBehaviour
{
    [SerializeField] private int amountOfItem;//掉落数量
    [SerializeField] private ItemData[] Drop;
    private List<ItemData> dropList = new List<ItemData>();//掉落物品设置

    [SerializeField] private GameObject dropPrefab;
    private ItemData item;//掉落物
    [SerializeField] private Animator animator;
    private bool isOpen = false;//是否打开
    private bool hasDropped = false;
    public void GenerateDrop()
    {
        for(int i = 0; i < Drop.Length; i++)
        {
           dropList.Add(Drop[i]);//添加要掉落的物品          
        }
        if(dropList.Count == 0)
        {
            Debug.Log("本次没有物品掉落");
        }

        for(int i = 0;i < amountOfItem; i++)
        {
            ItemData randomItem = dropList[i];
            dropList.Remove(randomItem);
            DropItem(randomItem);//掉落物品
            if(dropList.Count == 0)
            {
                break;
            }
        }

    }

    public void DropItem(ItemData itemdata)
    {
        GameObject newDrop = Instantiate(dropPrefab,transform.position, Quaternion.identity);//设置掉落组件并获取

        Vector2 randomVelocity = new Vector2(UnityEngine.Random.Range(-8,8), UnityEngine.Random.Range(15,20));//随机化掉落速度

        newDrop.GetComponent<ItemObject>().SetupItem(itemdata,randomVelocity);//弹出组件
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(isOpen)
        {
            return;
        }
        if (collision.GetComponent<Player>()!= null && Input.GetKeyDown(KeyCode.F))
        {
            isOpen = true;
            animator.SetBool("Open", true);
            // Drop will be performed by animation event calling DropAfterAnimation()
        }
    }

    private void DropAfterAnimation()
    {
        if (hasDropped) return;
        hasDropped = true;
        GenerateDrop();
    }
}
