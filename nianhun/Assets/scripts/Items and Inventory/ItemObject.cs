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
        if (rb != null)
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
            BounceAndNotify("背包已满！");
            return;
        }

        if (itemData.itemtype == ItemType.Material && !Inventory.instance.CanAddStashItem())
        {
            BounceAndNotify("材料仓库已满！");
            return;
        }

        Inventory.instance.AddItem(itemData);

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(8, null);

        string itemName = string.IsNullOrEmpty(itemData.itemname) ? itemData.name : itemData.itemname;
        ShowPopup("获得 " + itemName);

        Destroy(gameObject);
    }

    private void BounceAndNotify(string message)
    {
        if (rb != null)
            rb.velocity = new Vector2(0, 7);
        ShowPopup(message);
    }

    private static void ShowPopup(string message)
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        EntityFx fx = playermanger.instance.player.fx;
        if (fx != null)
            fx.CreatePopUpText(message);
    }
}
