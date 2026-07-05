#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveEquipmentEditorWindow : EditorWindow
{
    private const string DefaultSaveFileName = "nianhundata";
    private const string DatabasePath = "Assets/item/ItemDatabase.asset";
    private const string SaveManagerPrefabPath = "Assets/Prefabs/管理器/存档管理器.prefab";

    private ItemDataEquipment selectedEquipment;
    private bool equipDirectly = true;
    private int stackSize = 1;
    private bool encryptData = true;
    private string saveFileName = DefaultSaveFileName;
    private string statusMessage = string.Empty;
    private Vector2 equipmentScroll;
    private Vector2 saveScroll;
    private List<ItemDataEquipment> allEquipment = new List<ItemDataEquipment>();

    [MenuItem("Nianhun/存档/添加装备到存档")]
    public static void OpenWindow()
    {
        SaveEquipmentEditorWindow window = GetWindow<SaveEquipmentEditorWindow>("存档添加装备");
        window.minSize = new Vector2(420f, 520f);
        window.SyncSaveSettingsFromPrefab();
        window.RefreshEquipmentList();
    }

    private void OnEnable()
    {
        SyncSaveSettingsFromPrefab();
        RefreshEquipmentList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("向存档添加装备", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "写入本机 persistentDataPath 下的 nianhundata 存档。添加后请「继续游戏」或运行中点「重新加载存档」生效。",
            MessageType.Info);

        DrawSaveSettings();
        EditorGUILayout.Space(8f);
        DrawEquipmentPicker();
        EditorGUILayout.Space(8f);
        DrawAddOptions();
        EditorGUILayout.Space(8f);
        DrawCurrentSaveEquipment();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(statusMessage, MessageType.None);
        }
    }

    private void DrawSaveSettings()
    {
        EditorGUILayout.LabelField("存档设置", EditorStyles.boldLabel);
        saveFileName = EditorGUILayout.TextField("文件名", saveFileName);
        encryptData = EditorGUILayout.Toggle("加密", encryptData);

        string savePath = GetSavePath();
        EditorGUILayout.LabelField("路径", savePath, EditorStyles.wordWrappedMiniLabel);

        bool saveExists = File.Exists(savePath);
        EditorGUILayout.LabelField("存档存在", saveExists ? "是" : "否");

        if (saveExists && !TryLoadSaveData(out _))
            EditorGUILayout.HelpBox("存档存在但读取失败，请确认「加密」选项与游戏里一致（默认应勾选）。", MessageType.Warning);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("打开存档目录"))
            EditorUtility.RevealInFinder(savePath);
        if (GUILayout.Button("刷新装备列表"))
            RefreshEquipmentList();
        if (GUILayout.Button("同步游戏默认设置"))
            SyncSaveSettingsFromPrefab();
        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying && GUILayout.Button("重新加载存档（运行中）"))
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.ReloadFromDisk();
                statusMessage = "已从磁盘重新加载存档。";
            }
            else
                statusMessage = "运行中找不到 SaveManager。";
        }
    }

    private void DrawEquipmentPicker()
    {
        EditorGUILayout.LabelField("选择装备", EditorStyles.boldLabel);
        selectedEquipment = (ItemDataEquipment)EditorGUILayout.ObjectField(
            "装备",
            selectedEquipment,
            typeof(ItemDataEquipment),
            false);

        equipmentScroll = EditorGUILayout.BeginScrollView(equipmentScroll, GUILayout.Height(160f));
        foreach (ItemDataEquipment equipment in allEquipment)
        {
            if (equipment == null)
                continue;

            bool isSelected = selectedEquipment == equipment;
            GUIStyle style = isSelected ? EditorStyles.toolbarButton : EditorStyles.miniButton;
            if (GUILayout.Button($"{equipment.itemname} ({equipment.equipmenttype})", style))
                selectedEquipment = equipment;
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawAddOptions()
    {
        EditorGUILayout.LabelField("添加方式", EditorStyles.boldLabel);
        equipDirectly = EditorGUILayout.Toggle("直接穿戴", equipDirectly);

        if (!equipDirectly)
            stackSize = Mathf.Max(1, EditorGUILayout.IntField("数量", stackSize));

        EditorGUI.BeginDisabledGroup(selectedEquipment == null);
        if (GUILayout.Button("添加到存档", GUILayout.Height(32f)))
            AddEquipmentToSave();
        EditorGUI.EndDisabledGroup();
    }

    private void DrawCurrentSaveEquipment()
    {
        EditorGUILayout.LabelField("当前存档中的装备", EditorStyles.boldLabel);

        if (!TryLoadSaveData(out GameData data))
        {
            EditorGUILayout.LabelField("（无法读取存档）");
            return;
        }

        ItemDatabase database = LoadDatabase();
        if (database == null)
        {
            EditorGUILayout.LabelField("（找不到 ItemDatabase）");
            return;
        }

        saveScroll = EditorGUILayout.BeginScrollView(saveScroll, GUILayout.Height(140f));

        EditorGUILayout.LabelField("已穿戴", EditorStyles.miniBoldLabel);
        if (data.equipmentID == null || data.equipmentID.Count == 0)
        {
            EditorGUILayout.LabelField("  （无）");
        }
        else
        {
            foreach (string itemId in data.equipmentID)
            {
                if (database.TryGetItem(itemId, out ItemData item))
                    EditorGUILayout.LabelField($"  • {item.itemname}");
                else
                    EditorGUILayout.LabelField($"  • 未知 ID: {itemId}");
            }
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("背包/仓库（装备类）", EditorStyles.miniBoldLabel);
        bool hasInventoryEquipment = false;
        if (data.inventory != null)
        {
            foreach (KeyValuePair<string, int> pair in data.inventory)
            {
                if (!database.TryGetItem(pair.Key, out ItemData item) || item is not ItemDataEquipment)
                    continue;

                hasInventoryEquipment = true;
                EditorGUILayout.LabelField($"  • {item.itemname} × {pair.Value}");
            }
        }

        if (!hasInventoryEquipment)
            EditorGUILayout.LabelField("  （无）");

        EditorGUILayout.EndScrollView();
    }

    private void AddEquipmentToSave()
    {
        if (selectedEquipment == null)
        {
            statusMessage = "请先选择一件装备。";
            return;
        }

        if (string.IsNullOrEmpty(selectedEquipment.itemId))
        {
            statusMessage = $"装备「{selectedEquipment.itemname}」缺少 itemId，请在 Inspector 中重新保存该资源。";
            return;
        }

        ItemDatabase database = LoadDatabase();
        if (database == null)
        {
            statusMessage = "找不到 ItemDatabase，请先执行 Nianhun/Rebuild Item Database。";
            return;
        }

        if (!database.TryGetItem(selectedEquipment.itemId, out _))
        {
            statusMessage = $"「{selectedEquipment.itemname}」不在 ItemDatabase 中，请先执行 Nianhun/Rebuild Item Database。";
            return;
        }

        string savePath = GetSavePath();
        GameData data;
        if (File.Exists(savePath))
        {
            if (!TryLoadSaveData(out data))
            {
                statusMessage = "存档文件存在但无法读取，请检查「加密」选项是否与游戏一致，未修改存档。";
                return;
            }
        }
        else
        {
            data = new GameData();
        }

        if (equipDirectly)
            EquipInSaveData(data, selectedEquipment, database);
        else
            AddToInventoryInSaveData(data, selectedEquipment.itemId, stackSize);

        SaveSaveData(data);

        if (Application.isPlaying && SaveManager.instance != null)
            SaveManager.instance.ReloadFromDisk();

        statusMessage = equipDirectly
            ? $"已将「{selectedEquipment.itemname}」写入存档并设为已穿戴。{(Application.isPlaying ? " 已重新加载。" : " 请继续游戏查看。")}"
            : $"已将「{selectedEquipment.itemname}」× {stackSize} 写入存档背包。{(Application.isPlaying ? " 已重新加载。" : " 请继续游戏查看。")}";

        Repaint();
    }

    private static void EquipInSaveData(GameData data, ItemDataEquipment equipment, ItemDatabase database)
    {
        if (data.equipmentID == null)
            data.equipmentID = new List<string>();

        RemoveEquippedOfSameType(data, equipment.equipmenttype, database);

        if (!data.equipmentID.Contains(equipment.itemId))
            data.equipmentID.Add(equipment.itemId);
    }

    private static void RemoveEquippedOfSameType(GameData data, EquipmentType type, ItemDatabase database)
    {
        List<string> toRemove = new List<string>();

        foreach (string itemId in data.equipmentID)
        {
            if (!database.TryGetItem(itemId, out ItemData item) || item is not ItemDataEquipment equipped)
                continue;

            if (equipped.equipmenttype == type)
                toRemove.Add(itemId);
        }

        foreach (string itemId in toRemove)
            data.equipmentID.Remove(itemId);
    }

    private static void AddToInventoryInSaveData(GameData data, string itemId, int amount)
    {
        if (data.inventory == null)
            data.inventory = new SerializableDictionary<string, int>();

        if (data.inventory.TryGetValue(itemId, out int current))
            data.inventory[itemId] = current + amount;
        else
            data.inventory[itemId] = amount;
    }

    private bool TryLoadSaveData(out GameData data)
    {
        data = LoadSaveData();
        return data != null;
    }

    private GameData LoadSaveData()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
            return null;

        GameData loaded = CreateHandler(encryptData).Load();
        if (loaded != null)
            return loaded;

        if (!encryptData)
            return CreateHandler(true).Load();

        return CreateHandler(false).Load();
    }

    private void SaveSaveData(GameData data)
    {
        CreateHandler(encryptData).Save(data);
    }

    private FileDataHandler CreateHandler(bool encrypted)
    {
        return new FileDataHandler(Application.persistentDataPath, saveFileName, encrypted);
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    private void SyncSaveSettingsFromPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SaveManagerPrefabPath);
        if (prefab == null)
            return;

        SaveManager saveManager = prefab.GetComponent<SaveManager>();
        if (saveManager == null)
            return;

        saveFileName = saveManager.SaveFileName;
        encryptData = saveManager.EncryptData;
    }

    private void RefreshEquipmentList()
    {
        allEquipment.Clear();

        ItemDatabase database = LoadDatabase();
        if (database == null)
            return;

        database.BuildLookup();

        string[] guids = AssetDatabase.FindAssets("t:ItemDataEquipment", new[] { "Assets/item/items" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataEquipment equipment = AssetDatabase.LoadAssetAtPath<ItemDataEquipment>(path);
            if (equipment != null)
                allEquipment.Add(equipment);
        }

        allEquipment.Sort((a, b) => string.Compare(a.itemname, b.itemname, System.StringComparison.Ordinal));
    }

    private static ItemDatabase LoadDatabase()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabasePath);
        database?.BuildLookup();
        return database;
    }
}
#endif
