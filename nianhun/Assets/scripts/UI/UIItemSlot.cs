using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image itemimage;
    [SerializeField] protected TextMeshProUGUI itemtext;

    public InventoryItem item;
    protected UI ui;

    protected virtual void Start()
    {
        ui = GetComponentInParent<UI>();
        if (item != null && (item.data == null || itemimage == null))
            CleanUpSlot();
    }

    public void UpdateSlot(InventoryItem newitem)
    {
        if (newitem == null || newitem.data == null || itemimage == null)
        {
            CleanUpSlot();
            return;
        }

        item = newitem;
        itemimage.sprite = newitem.data.icon;

        if (newitem.data.icon == null)
        {
            itemimage.color = Color.clear;
        }
        else
        {
            itemimage.color = Color.white;
        }

        if (itemtext != null)
            itemtext.text = newitem.stackSize > 1 ? newitem.stackSize.ToString() : string.Empty;
    }

    public void CleanUpSlot()
    {
        item = null;

        if (itemimage != null)
        {
            itemimage.sprite = null;
            itemimage.color = Color.clear;
        }

        if (itemtext != null)
            itemtext.text = string.Empty;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null || item.data == null || Inventory.instance == null)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Inventory.instance.RemoveItem(item.data);
            return;
        }

        if (item.data.itemtype == ItemType.Equipment)
            Inventory.instance.EquipItem(item.data);

        if (ui != null && ui.ItemTooltip != null)
            ui.ItemTooltip.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || item.data == null || ui == null || ui.ItemTooltip == null)
            return;

        ui.ItemTooltip.ShowTooltip(item.data as ItemDataEquipment);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui == null || ui.ItemTooltip == null)
            return;

        ui.ItemTooltip.HideTooltip();
    }
}
