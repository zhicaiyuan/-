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
            if (!HasCraftMaterials(craftEquipment[i]))
            {
                continue;
            }
            GameObject newSlot = Instantiate(craftSlotPerfab, craftSlotParent);
            newSlot.GetComponent<UICraftSlot>().SetupCraftSlot(craftEquipment[i]);
        }
    }//切换时设置工艺列表

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance.PlaySFX(14, null);
        SetupCraftList();
    }

    public void SetupDefaultCraftWindow()
    {
        ItemDataEquipment defaultRecipe = GetFirstCraftableEquipment();
        if (defaultRecipe != null)
            GetComponentInParent<UI>().craftwindow.SetupCraftWindow(defaultRecipe);
    }

    private ItemDataEquipment GetFirstCraftableEquipment()
    {
        foreach (ItemDataEquipment equipment in craftEquipment)
        {
            if (HasCraftMaterials(equipment))
                return equipment;
        }

        return null;
    }

    public bool HasCraftMaterials(ItemDataEquipment data)
    {
        if(data == null || data.craftingMaterials == null || data.craftingMaterials.Count == 0)
        {
            return false;
        }
        foreach(InventoryItem item in data.craftingMaterials)
        {
            if(item != null && item.data != null && item.stackSize > 0)
            {
                return true;
            }
        }
        return false;
    }
}
