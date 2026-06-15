using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "物品/物品数据库")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> lookupById;

    public void BuildLookup()
    {
        lookupById = new Dictionary<string, ItemData>(items.Count);

        foreach (ItemData item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId))
                continue;

            lookupById[item.itemId] = item;
        }
    }

    public bool TryGetItem(string itemId, out ItemData item)
    {
        if (lookupById == null)
            BuildLookup();

        return lookupById.TryGetValue(itemId, out item);
    }

#if UNITY_EDITOR
    public List<ItemData> EditorItems => items;

    public void EditorSetItems(List<ItemData> newItems)
    {
        items = newItems;
        lookupById = null;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
