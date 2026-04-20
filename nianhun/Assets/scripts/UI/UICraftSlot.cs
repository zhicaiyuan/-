using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICraftSlot : UIItemSlot
{
    protected override void Start()
    {
        base.Start();
    }

    public void SetupCraftSlot(ItemDataEquipment data)
    {
        if(data == null)
            return;

        item.data = data;

        itemimage.sprite = data.icon;
        itemtext.text = data.itemname;
        itemtext.fontSize = 36;

        if (itemtext.text.Length > 4)
            itemtext.fontSize = itemtext.fontSize * .7f;

    }//设置图片与文字
    public override void OnPointerDown(PointerEventData eventData)
    {
        ui.craftwindow.SetupCraftWindow(item.data as ItemDataEquipment);
    }
}
