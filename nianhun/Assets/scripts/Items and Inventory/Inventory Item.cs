
using UnityEngine;
[System.Serializable]
public class InventoryItem 
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData newitemdata)
    {
        data = newitemdata;
        AddStack();
    }//转化函数

    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}
