using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class ItemObject : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;

    

    private bool SetupVisuals()
    {
        if (itemData == null)
            return false;
        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "物品" + itemData.name;
        return true;
    }//显示图像

    

    public  void SetupItem(ItemData _itemdata,Vector2 _velocity)
    {
        itemData = _itemdata;
        rb.velocity = _velocity;

        bool flowControl = SetupVisuals();
        if (!flowControl)
        {
            return;
        }
    }//设置物品弹射
    public void PickUpItem()
    {
        if (!Inventory.instance.CanAddItem() && itemData.itemtype == ItemType.Equipment)//如果装备满了就返回{
        {
            rb.velocity = new Vector2(0, 7);
            return;
        }
        Inventory.instance.AddItem(itemData);
        UnityEngine.Object.Destroy(gameObject);
    }//捡起然后摧毁物品
}
