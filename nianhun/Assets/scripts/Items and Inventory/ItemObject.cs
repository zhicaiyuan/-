using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;

    private bool SetupVisuals()
    {
        if (itemData == null)
            return false;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return false;

        spriteRenderer.sprite = itemData.icon;
        gameObject.name = "物品" + itemData.name;
        return true;
    }

    public void SetupItem(ItemData _itemdata, Vector2 _velocity)
    {
        itemData = _itemdata;
        rb.velocity = _velocity;

        if (!SetupVisuals())
            return;
    }

    public void PickUpItem()
    {
        if (itemData == null || Inventory.instance == null)
            return;

        if (itemData.itemtype == ItemType.Equipment && !Inventory.instance.CanAddItem())
        {
            rb.velocity = new Vector2(0, 7);
            playermanger.instance.player.fx.CreatePopUpText("背包已满！");
            return;
        }

        if (itemData.itemtype == ItemType.Material && !Inventory.instance.CanAddStashItem())
        {
            rb.velocity = new Vector2(0, 7);
            playermanger.instance.player.fx.CreatePopUpText("材料仓库已满！");
            return;
        }

        Inventory.instance.AddItem(itemData);
        AudioManager.instance.PlaySFX(8, null);
        playermanger.instance.player.fx.CreatePopUpText("获得 " + itemData.itemname);
        Destroy(gameObject);
    }
}
