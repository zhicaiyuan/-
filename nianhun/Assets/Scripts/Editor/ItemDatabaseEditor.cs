#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ItemDatabaseEditor
{
    private const string DatabasePath = "Assets/item/ItemDatabase.asset";
    private const string ItemsFolder = "Assets/item/items";

    [MenuItem("Nianhun/Rebuild Item Database")]
    public static void RebuildItemDatabase()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabasePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        List<ItemData> items = new List<ItemData>();
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { ItemsFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null)
                items.Add(item);
        }

        database.EditorSetItems(items);
        database.BuildLookup();
        AssetDatabase.SaveAssets();

        AssignDatabaseToInventories(database);
        Debug.Log($"ItemDatabase rebuilt with {items.Count} items.");
    }

    private static void AssignDatabaseToInventories(ItemDatabase database)
    {
        foreach (Inventory inventory in Object.FindObjectsOfType<Inventory>(true))
        {
            SerializedObject serializedInventory = new SerializedObject(inventory);
            SerializedProperty databaseProperty = serializedInventory.FindProperty("itemDatabase");
            if (databaseProperty == null)
                continue;

            databaseProperty.objectReferenceValue = database;
            serializedInventory.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventory);
        }
    }
}
#endif
