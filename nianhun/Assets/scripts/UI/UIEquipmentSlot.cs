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
        if (item == null || item.data == null || Inventory.instance == null)
            return;

        ItemDataEquipment equipment = item.data as ItemDataEquipment;
        if (equipment == null)
            return;

        AudioManager.instance.PlaySFX(15, null);
        Inventory.instance.Unequipitem(equipment);
        Inventory.instance.AddItem(equipment);

        if (ui != null && ui.ItemTooltip != null)
            ui.ItemTooltip.HideTooltip();

        CleanUpSlot();
    }
}
