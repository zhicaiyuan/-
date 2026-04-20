using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> startingEquipment;
    public List<ItemData> startingMaterial;

    public List<InventoryItem> inventory;
    public Dictionary<ItemData, InventoryItem> inventoryDictianory;//键值类管理材料

    public List<InventoryItem> stash;
    public Dictionary<ItemData, InventoryItem> stashDictianory;//同库存用于储存装备

    public List<InventoryItem> equipment;
    public Dictionary<ItemDataEquipment, InventoryItem> equipmentDictianory;//用于显示装备装备

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform stashSlotParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform statSlotParent;//位置

    [Header("Items cooldown")]
    private float lastTimeofUsedFlask;
    private float lastTimeofUsedArmor;
    private float flaskCooldown;
    private float ArmorCooldown;

    private UIItemSlot[] inventoryitemSlot;
    private UIItemSlot[] stashitemslot;
    private UIEquipmentSlot[] equipmentSlot;
    private UIStatSlot[] statSlot;//关联物品
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            UnityEngine.Object.Destroy(gameObject);
    }//防止物件重复

    private void Start()
    {
        inventory = new List<InventoryItem>();
        inventoryDictianory = new Dictionary<ItemData, InventoryItem>();//分配组件

        stash = new List<InventoryItem>();
        stashDictianory = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictianory = new Dictionary<ItemDataEquipment, InventoryItem>();

        inventoryitemSlot = inventorySlotParent.GetComponentsInChildren<UIItemSlot>();
        stashitemslot = stashSlotParent.GetComponentsInChildren<UIItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UIEquipmentSlot>();
        statSlot = statSlotParent.GetComponentsInChildren<UIStatSlot>();//从父类获取

        AddStartingEquipment();

        AddStartingMaterial();
        void AddStartingEquipment()
        {
            for (int i = 0; i < startingEquipment.Count; i++)
            {
                AddItem(startingEquipment[i]);
            }
        }//添加初始装备
        void AddStartingMaterial()
        {
            for (int i = 0; i < startingMaterial.Count; i++)
            {
                AddItem(startingMaterial[i]);
            }
        }//添加初始材料
    }

    public void EquipItem(ItemData item)
    {
        ItemDataEquipment newEquipment = item as ItemDataEquipment;
        InventoryItem newitem = new InventoryItem(newEquipment);//转化类型

        ItemDataEquipment oldequipment = null;

        foreach (KeyValuePair<ItemDataEquipment, InventoryItem> _item in equipmentDictianory)//遍历装备
        {
            if (_item.Key.equipmenttype == newEquipment.equipmenttype)//如果装备重复
            {
                oldequipment = _item.Key;//标记
            }
        }

        if (oldequipment != null)
        {
            Unequipitem(oldequipment);
            AddItem(oldequipment);
        }
        equipment.Add(newitem);
        equipmentDictianory.Add(newEquipment, newitem);
        newEquipment.AddModifiers();

        RemoveItem(item);

        UpdateSlotUI();

    }
    public void Unequipitem(ItemDataEquipment itemToDelete)//删除重复的标记项
    {
        if (equipmentDictianory.TryGetValue(itemToDelete, out InventoryItem value))
        {
            equipment.Remove(value);
            equipmentDictianory.Remove(itemToDelete);
            itemToDelete.RemoveModifiers();

        }
    }

    private void UpdateSlotUI()
    {
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemDataEquipment, InventoryItem> _item in equipmentDictianory)
            {
                if (_item.Key.equipmenttype == equipmentSlot[i].slottype)
                {
                    equipmentSlot[i].UpdateSlot(_item.Value);
                }
            }
        }

        for (int i = 0; i < inventoryitemSlot.Length; i++)
        {
            inventoryitemSlot[i].CleanUpSlot();
        }

        for (int i = 0; i < stashitemslot.Length; i++)
        {
            stashitemslot[i].CleanUpSlot();
        }//清理

        for (int i = 0; i < inventory.Count; i++)
        {
            inventoryitemSlot[i].UpdateSlot(inventory[i]);
        }
        for (int i = 0; i < stash.Count; i++)
        {
            stashitemslot[i].UpdateSlot(stash[i]);
        }//添加
        for(int i = 0; i < statSlot.Length; i++)
        {
            statSlot[i].UpdateStatValueUI();
        }//更新角色属性面板
    }//更新ui

    public void AddItem(ItemData item)
    {
        if (item.itemtype == ItemType.Equipment && CanAddItem())//如果类型为装备
            Addtoinventory(item);
        else if (item.itemtype == ItemType.Material)//如果是材料      
            Addtostash(item);

        UpdateSlotUI();

        void Addtoinventory(ItemData item)
        {
            if (inventoryDictianory.TryGetValue(item, out InventoryItem value))//尝试取值
            {
                value.AddStack();
            }
            else
            {
                InventoryItem newitem = new InventoryItem(item);//没有就调用函数新建词条
                inventory.Add(newitem);
                inventoryDictianory.Add(item, newitem);
            }
        }

        void Addtostash(ItemData item)
        {
            if (stashDictianory.TryGetValue(item, out InventoryItem value))//尝试取值
            {
                value.AddStack();
            }
            else
            {
                InventoryItem newitem = new InventoryItem(item);
                stash.Add(newitem);
                stashDictianory.Add(item, newitem);
            }
        }
    }//添加物品

    public void RemoveItem(ItemData item)
    {
        if (inventoryDictianory.TryGetValue(item, out InventoryItem value))
        {
            if (value.stackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictianory.Remove(item);//移除词条
            }
            else
                value.RemoveStack();
        }
        if (stashDictianory.TryGetValue(item, out InventoryItem stashvalue))
        {
            if (stashvalue.stackSize <= 1)
            {
                stash.Remove(stashvalue);
                stashDictianory.Remove(item);
            }
            else
                stashvalue.RemoveStack();
        }

        UpdateSlotUI();
    }//移除物品

    public bool CanCraft(ItemDataEquipment itemtoCraft, List<InventoryItem> requireMaterials)
    {
        List<InventoryItem> mertialsToRemove = new List<InventoryItem>();//添加材料列表判断
        for (int i = 0; i < requireMaterials.Count; i++)
        {
            if (stashDictianory.TryGetValue(requireMaterials[i].data, out InventoryItem stashvalue))//是否存在相关对象
            {
                if (stashvalue.stackSize < requireMaterials[i].stackSize)
                {
                    Debug.Log("没有足够材料");
                    return false;
                }
                else
                {
                    mertialsToRemove.Add(stashvalue);//增加添加对象
                }
            }
            else
            {
                Debug.Log("没有足够材料");
                return false;
            }
        }

        for (int i = 0; i < mertialsToRemove.Count; i++)
        {
            for (int j = 0; j < mertialsToRemove[i].stackSize; j++)
            {
                
            RemoveItem(mertialsToRemove[i].data);//删除制造材料
            }
        }
        AddItem(itemtoCraft);
        Debug.Log("制造物品" + itemtoCraft.name);

        return true;

    }//判断是否可以制造

    public List<InventoryItem> GetEquipmentList() => equipment;

    public List<InventoryItem> GetStashList() => stash;

    public ItemDataEquipment GetEquipment(EquipmentType type)
    {
        ItemDataEquipment equipedItemData = null;
        foreach (KeyValuePair<ItemDataEquipment, InventoryItem> _item in equipmentDictianory)//遍历装备
        {
            if (_item.Key.equipmenttype == type)//如果装备是对应目标
            {
                equipedItemData = _item.Key;
            }
        }

        return equipedItemData;
    }//获取装备

    public void UseFlask()
    {
        ItemDataEquipment currentFlask = GetEquipment(EquipmentType.道具);

        if (currentFlask == null)
            return;

        bool canUseFlask = Time.time > lastTimeofUsedFlask + flaskCooldown;//判断是否可以使用

        if (canUseFlask)
        {
            flaskCooldown = currentFlask.itemCooldown;
            currentFlask.Effect(null);//使用物品
            lastTimeofUsedFlask = Time.time;
        }
        else
            Debug.Log("道具正在冷却");
    }//判断是否可以用道具

    public bool CanAddItem()
    {
        if(inventory.Count >= inventoryitemSlot.Length)
        {
            Debug.Log("背包空间不足");
            return false;
        }

        return true;
    }


    public bool CanUseArmor()
    {
        ItemDataEquipment currentArmor = GetEquipment(EquipmentType.护甲);

        if(Time.time > lastTimeofUsedArmor + ArmorCooldown)
        {
            ArmorCooldown = currentArmor.itemCooldown;
            lastTimeofUsedArmor = Time.time;
            return true;
        }

        Debug.Log("护甲技能冷却中");
        return false;
    }//判断是否可以用护甲技能

    
}

