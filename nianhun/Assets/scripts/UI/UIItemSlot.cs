using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class UIItemSlot : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler//分别对应按下，鼠标指到和鼠标离开的接口
{
    [SerializeField]protected Image itemimage;
    [SerializeField]protected TextMeshProUGUI itemtext;

    public InventoryItem item;
    protected UI ui;

    protected virtual void Start()
    {
        ui = GetComponentInParent<UI>();
    }
    public void UpdateSlot(InventoryItem newitem)
    {
        
        item = newitem;

        itemimage.color = Color.white;

        if (itemimage != null)
        {
            itemimage.sprite = newitem.data.icon;

            if (newitem.stackSize > 1)
            {
                itemtext.text = newitem.stackSize.ToString();
            }
            else
            {
                itemtext.text = "";
            }
        }//设置物品数量和图形
    }

    public void CleanUpSlot()
    {
        item =null;
        itemimage.sprite = null;
        itemimage.color = Color.clear;
        itemtext.text = "";
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (item == null)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Inventory.instance.RemoveItem(item.data);
            return;
        }

        if(item.data.itemtype == ItemType.Equipment)
            Inventory.instance.EquipItem(item.data);
        

        ui.ItemTooltip.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(item == null)
            return;

        Debug.Log("展示物品信息");
        ui.ItemTooltip.ShowTooltip(item.data as ItemDataEquipment);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(item == null)
            return ;
        Debug.Log("隐藏物品信息");
        ui.ItemTooltip.HideTooltip();
    }
}
