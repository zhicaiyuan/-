using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEquipmentSlot : UIItemSlot
{
    public EquipmentType slottype;

    private void OnValidate()
    {
        gameObject.name = "装备栏 - " + slottype.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(item == null || item.data == null)
            return;
        Inventory.instance.Unequipitem(item.data as ItemDataEquipment);//从装备里解除
        Inventory.instance.AddItem(item.data as ItemDataEquipment);//从库存里添加

        ui.ItemTooltip.HideTooltip();

        CleanUpSlot();
    }
}
