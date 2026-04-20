using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICraftLIst : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private Transform craftSlotParent;
    [SerializeField] private GameObject craftSlotPerfab;

    [SerializeField] private List<ItemDataEquipment> craftEquipment;
    
    void Start()
    {
        transform.parent.GetChild(0).GetComponent<UICraftLIst>().SetupCraftList();
        SetupDefaultCraftWindow();//默认设置为列表的第一个
    }

    

    public void SetupCraftList()
    {
        for (int i = 0; i < craftSlotParent.childCount; i++)
        {
            Destroy(craftSlotParent.GetChild(i).gameObject);
        }

        

        for (int i = 0; i < craftEquipment.Count; i++)
        {
            GameObject newSlot = Instantiate(craftSlotPerfab, craftSlotParent);
            newSlot.GetComponent<UICraftSlot>().SetupCraftSlot(craftEquipment[i]);
        }
    }//切换时设置工艺列表

    public void OnPointerDown(PointerEventData eventData)
    {
        SetupCraftList();
    }

    public void SetupDefaultCraftWindow()
    {
        if (craftEquipment[0] != null) 
            GetComponentInParent<UI>().craftwindow.SetupCraftWindow(craftEquipment[0]);
    }
}
