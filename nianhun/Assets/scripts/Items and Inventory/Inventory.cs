using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Inventory : MonoBehaviour,ISaveManager
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
    public float flaskCooldown {  get; private set; }
    private float ArmorCooldown;

    private UIItemSlot[] inventoryitemSlot;
    private UIItemSlot[] stashitemslot;
    private UIEquipmentSlot[] equipmentSlot;
    private UIStatSlot[] statSlot;//关联物品

    [Header("Data base")]
    [SerializeField] private ItemDatabase itemDatabase;
    public List<InventoryItem> loadedItems;
    public List<ItemDataEquipment> loadedEquipments;

    private bool hasInitialized;
    private bool hasAppliedLoadedSave;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            UnityEngine.Object.Destroy(gameObject);

        EnsureItemDatabase();
    }//防止物件重复

    private void Start()
    {
        inventory = new List<InventoryItem>();
        inventoryDictianory = new Dictionary<ItemData, InventoryItem>();

        stash = new List<InventoryItem>();
        stashDictianory = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictianory = new Dictionary<ItemDataEquipment, InventoryItem>();

        InitializeSlotUI();

        hasInitialized = true;
        hasAppliedLoadedSave = false;
        ApplyLoadedSaveData();
    }

    private void TryAutoResolveSlotParents()
    {
        if (inventorySlotParent != null && stashSlotParent != null &&
            equipmentSlotParent != null && statSlotParent != null)
            return;

        UI ui = FindObjectOfType<UI>(true);
        if (ui == null)
            return;

        UIEquipmentSlot[] equipSlots = ui.GetComponentsInChildren<UIEquipmentSlot>(true);
        if (equipmentSlotParent == null && equipSlots.Length > 0)
            equipmentSlotParent = equipSlots[0].transform.parent;

        UIStatSlot[] statSlots = ui.GetComponentsInChildren<UIStatSlot>(true);
        if (statSlotParent == null && statSlots.Length > 0)
            statSlotParent = statSlots[0].transform.parent;

        Dictionary<Transform, int> plainSlotCounts = new Dictionary<Transform, int>();
        foreach (UIItemSlot slot in ui.GetComponentsInChildren<UIItemSlot>(true))
        {
            if (slot is UICraftSlot || slot is UIEquipmentSlot)
                continue;

            Transform parent = slot.transform.parent;
            if (parent == null)
                continue;

            plainSlotCounts.TryGetValue(parent, out int count);
            plainSlotCounts[parent] = count + 1;
        }

        foreach (KeyValuePair<Transform, int> pair in plainSlotCounts)
        {
            string parentName = pair.Key.name;

            if (inventorySlotParent == null &&
                (parentName.Contains("库存") || parentName.Contains("装备")))
            {
                inventorySlotParent = pair.Key;
                continue;
            }

            if (stashSlotParent == null &&
                (parentName.Contains("储藏") || parentName.Contains("材料") || parentName.Contains("仓库")))
            {
                stashSlotParent = pair.Key;
            }
        }

        List<KeyValuePair<Transform, int>> sortedParents = new List<KeyValuePair<Transform, int>>(plainSlotCounts);
        sortedParents.Sort((a, b) => b.Value.CompareTo(a.Value));

        if (inventorySlotParent == null && sortedParents.Count > 0)
            inventorySlotParent = sortedParents[0].Key;

        if (stashSlotParent == null && sortedParents.Count > 1)
            stashSlotParent = sortedParents[1].Key;
    }

    private UIItemSlot[] CollectPlainItemSlots(Transform parent)
    {
        if (parent == null)
            return System.Array.Empty<UIItemSlot>();

        List<UIItemSlot> slots = new List<UIItemSlot>();
        foreach (UIItemSlot slot in parent.GetComponentsInChildren<UIItemSlot>(true))
        {
            if (slot is UICraftSlot || slot is UIEquipmentSlot)
                continue;

            slots.Add(slot);
        }

        return slots.ToArray();
    }

    private void EnsureSlotUIReady()
    {
        if (inventoryitemSlot != null && inventoryitemSlot.Length > 0 &&
            stashitemslot != null && stashitemslot.Length > 0)
            return;

        TryAutoResolveSlotParents();
        InitializeSlotUI();
    }

    private void InitializeSlotUI()
    {
        TryAutoResolveSlotParents();

        if (inventorySlotParent == null || stashSlotParent == null ||
            equipmentSlotParent == null || statSlotParent == null)
        {
            inventoryitemSlot = System.Array.Empty<UIItemSlot>();
            stashitemslot = System.Array.Empty<UIItemSlot>();
            equipmentSlot = System.Array.Empty<UIEquipmentSlot>();
            statSlot = System.Array.Empty<UIStatSlot>();
            Debug.LogWarning("Inventory UI 未配置，已跳过背包界面初始化。", this);
            return;
        }

        inventoryitemSlot = CollectPlainItemSlots(inventorySlotParent);
        stashitemslot = CollectPlainItemSlots(stashSlotParent);
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UIEquipmentSlot>(true);
        statSlot = statSlotParent.GetComponentsInChildren<UIStatSlot>(true);
    }
    
    

    public void EquipItem(ItemData item, bool playSound = true)
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

        if (playSound)
            AudioManager.instance.PlaySFX(15, null);

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
        if (equipmentSlot == null || inventoryitemSlot == null || stashitemslot == null || statSlot == null)
            return;

        if (equipmentSlot.Length == 0 && inventoryitemSlot.Length == 0 && stashitemslot.Length == 0)
            return;

        for (int i = 0; i < equipmentSlot.Length; i++)
            equipmentSlot[i].CleanUpSlot();

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
        if (item == null)
            return;

        EnsureSlotUIReady();

        bool added = false;

        if (item.itemtype == ItemType.Equipment && CanAddItem())
        {
            Addtoinventory(item);
            added = true;
        }
        else if (item.itemtype == ItemType.Material && CanAddStashItem())
        {
            Addtostash(item);
            added = true;
        }

        if (added)
            UpdateSlotUI();

        void Addtoinventory(ItemData item)
        {
            if (inventoryDictianory.TryGetValue(item, out InventoryItem value))
            {
                value.AddStack();
            }
            else
            {
                InventoryItem newitem = new InventoryItem(item);
                inventory.Add(newitem);
                inventoryDictianory.Add(item, newitem);
            }
        }

        void Addtostash(ItemData item)
        {
            if (stashDictianory.TryGetValue(item, out InventoryItem value))
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
        if (item == null)
            return;

        // 每次只扣 1 层，优先背包再材料库（避免同时从两边各扣一次）
        if (inventoryDictianory != null && inventoryDictianory.TryGetValue(item, out InventoryItem value))
        {
            if (value.stackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictianory.Remove(item);
            }
            else
                value.RemoveStack();

            UpdateSlotUI();
            return;
        }

        if (stashDictianory != null && stashDictianory.TryGetValue(item, out InventoryItem stashvalue))
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

    /// <summary>从背包与材料库合计检索物品数量。</summary>
    public int GetItemCount(ItemData item)
    {
        if (item == null)
            return 0;

        int count = 0;
        if (inventoryDictianory != null && inventoryDictianory.TryGetValue(item, out InventoryItem invItem))
            count += invItem.stackSize;
        if (stashDictianory != null && stashDictianory.TryGetValue(item, out InventoryItem stashItem))
            count += stashItem.stackSize;
        return count;
    }

    /// <summary>背包/材料库有足够数量则扣除并返回 true。</summary>
    public bool TryConsumeItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return true;

        if (GetItemCount(item) < amount)
            return false;

        for (int i = 0; i < amount; i++)
            RemoveItem(item);

        return true;
    }

    public bool CanCraft(ItemDataEquipment itemtoCraft, List<InventoryItem> requireMaterials)
    {


        // 检查装备类型是否有效
        if (itemtoCraft == null)
        {
            return false;
        }

        List<InventoryItem> mertialsToRemove = new List<InventoryItem>();
        for (int i = 0; i < requireMaterials.Count; i++)
        {
            if (stashDictianory.TryGetValue(requireMaterials[i].data, out InventoryItem stashvalue))
            {
                if (stashvalue.stackSize < requireMaterials[i].stackSize)
                {
                    playermanger.instance.player.fx.CreatePopUpText("没有足够材料！");
                    return false;
                }
                mertialsToRemove.Add(stashvalue);
            }
            else if (requireMaterials[i].data is ItemDataEquipment equipmentMaterial)
            {
                if (inventoryDictianory.TryGetValue(equipmentMaterial, out InventoryItem equipmentValue))
                {
                    if (equipmentValue.stackSize < requireMaterials[i].stackSize)
                    {
                        playermanger.instance.player.fx.CreatePopUpText("装备材料不足！");
                        return false;
                    }
                    mertialsToRemove.Add(equipmentValue);
                }
                else
                {
                    playermanger.instance.player.fx.CreatePopUpText("装备材料不足！");
                    return false;
                }
            }
            else
            {
                playermanger.instance.player.fx.CreatePopUpText("没有足够材料！");
                return false;
            }
        }

        for (int i = 0; i < mertialsToRemove.Count; i++)
        {
            for (int j = 0; j < mertialsToRemove[i].stackSize; j++)
            {
                RemoveItem(mertialsToRemove[i].data);
            }
        }

        AudioManager.instance.PlaySFX(2, null);
        AddItem(itemtoCraft);
        playermanger.instance.player.fx.CreatePopUpText("制作成功！");

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
            AudioManager.instance.PlaySFX(19, null);
            flaskCooldown = currentFlask.itemCooldown;
            currentFlask.Effect(null);//使用物品
            lastTimeofUsedFlask = Time.time;
        }
        else
            playermanger.instance.player.fx.CreatePopUpText("道具正在冷却！");
    }//判断是否可以用道具

    public bool CanAddStashItem()
    {
        if (stashitemslot == null || stashitemslot.Length == 0)
            return true;

        if (stash.Count >= stashitemslot.Length)
        {
            Debug.Log("材料仓库已满");
            return false;
        }

        return true;
    }

    public bool CanAddItem()
    {
        if (inventoryitemSlot == null || inventoryitemSlot.Length == 0)
            return true;

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

        playermanger.instance.player.fx.CreatePopUpText("护甲技能冷却中！");
        return false;
    }//判断是否可以用护甲技能

    public void LoadData(GameData data)
    {
        EnsureItemDatabase();

        if (itemDatabase == null)
        {
            Debug.LogError("Inventory 找不到 ItemDatabase，背包物品无法加载。", this);
            return;
        }

        loadedItems = new List<InventoryItem>();
        loadedEquipments = new List<ItemDataEquipment>();

        if (data.inventory != null)
        {
            foreach (KeyValuePair<string, int> pair in data.inventory)
            {
                if (!itemDatabase.TryGetItem(pair.Key, out ItemData item))
                {
                    Debug.LogWarning($"存档中的物品无法识别，已跳过: {pair.Key}", this);
                    continue;
                }

                InventoryItem itemToLoad = new InventoryItem(item);
                itemToLoad.stackSize = pair.Value;
                if (itemToLoad.stackSize > 0)
                    loadedItems.Add(itemToLoad);
            }
        }

        if (data.equipmentID != null)
        {
            foreach (string loadeditemId in data.equipmentID)
            {
                if (!itemDatabase.TryGetItem(loadeditemId, out ItemData item))
                {
                    Debug.LogWarning($"存档中的装备无法识别，已跳过: {loadeditemId}", this);
                    continue;
                }

                if (item is ItemDataEquipment equipment)
                    loadedEquipments.Add(equipment);
            }
        }

        if (!hasInitialized)
            return;

        ClearRuntimeInventory();
        hasAppliedLoadedSave = false;
        ApplyLoadedSaveData();
    }

    public void ApplyLoadedSaveData()
    {
        if (!hasInitialized || hasAppliedLoadedSave)
            return;

        hasAppliedLoadedSave = true;

        bool hasSaveInventory = loadedItems != null && loadedItems.Count > 0;
        bool hasSaveEquipment = loadedEquipments != null && loadedEquipments.Count > 0;

        if (hasSaveEquipment)
        {
            foreach (ItemDataEquipment item in loadedEquipments)
                EquipItem(item, playSound: false);
        }

        if (hasSaveInventory)
        {
            foreach (InventoryItem item in loadedItems)
            {
                for (int i = 0; i < item.stackSize; i++)
                    AddItem(item.data);
            }

            UpdateSlotUI();
            return;
        }

        if (hasSaveEquipment)
        {
            UpdateSlotUI();
            return;
        }

        if (startingEquipment != null)
        {
            for (int i = 0; i < startingEquipment.Count; i++)
                AddItem(startingEquipment[i]);
        }

        if (startingMaterial != null)
        {
            for (int i = 0; i < startingMaterial.Count; i++)
                AddItem(startingMaterial[i]);
        }

        UpdateSlotUI();
    }

    private void ClearRuntimeInventory()
    {
        if (equipmentDictianory != null)
        {
            List<ItemDataEquipment> equipped = new List<ItemDataEquipment>(equipmentDictianory.Keys);
            foreach (ItemDataEquipment item in equipped)
                Unequipitem(item);
        }

        inventory?.Clear();
        stash?.Clear();
        inventoryDictianory?.Clear();
        stashDictianory?.Clear();
        equipment?.Clear();
        equipmentDictianory?.Clear();
        UpdateSlotUI();
    }

    public void SaveData(ref GameData data)
    {
        data.inventory.Clear();
        data.equipmentID.Clear();

        foreach(KeyValuePair<ItemData,InventoryItem>pair in inventoryDictianory)
        {
            data.inventory.Add(pair.Key.itemId, pair.Value.stackSize);
        }//遍历每个字典添加物品id和数量

        foreach(KeyValuePair<ItemData,InventoryItem> pair  in stashDictianory)
        {
            data.inventory.Add(pair.Key.itemId,pair.Value.stackSize);
        }//同上添加储藏

        foreach(KeyValuePair<ItemDataEquipment,InventoryItem> pair in equipmentDictianory)
        {
            data.equipmentID.Add(pair.Key.itemId);
        }//添加身上的装备
    }

    private void EnsureItemDatabase()
    {
#if UNITY_EDITOR
        if (itemDatabase == null)
            itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/item/ItemDatabase.asset");
#endif
        if (itemDatabase == null)
            itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");

        itemDatabase?.BuildLookup();
    }
}

