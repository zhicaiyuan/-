
using System.Text;
using UnityEditor;
using UnityEngine;
public enum ItemType 
{
    Material,
    Equipment
}


[CreateAssetMenu(fileName ="新物品信息",menuName ="物品")]
public class ItemData : ScriptableObject
{
    public ItemType itemtype;
    public string itemname;
    public Sprite icon;
    public string itemId;

    [UnityEngine.Range(0,100)]
    public float dropchance;//定义物品组件

    protected StringBuilder sb = new StringBuilder();

    private void OnValidate()
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        itemId = AssetDatabase.AssetPathToGUID(path);
#endif
    }
    public virtual string GetDescription()
    {
        return "";
    }//通过字符串建造获取描述
}
