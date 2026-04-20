using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Stat 
{
    [SerializeField]private int basevalue;//公共基础值

    public List<int> modifiers;//附加值
    public int Getvalue()//获取数值函数
    {
        int finalvalue = basevalue;

        foreach(int modifier in modifiers)//遍历添加
        {
            finalvalue += modifier;
        }

        return finalvalue; 
    }

    public void setdefaultvalue(int value)
    {
        basevalue = value;
    }

    public void Addmodifier(int modifier)//添加函数
    {
        this.modifiers.Add(modifier); 
    }

    public void Removemodifier(int modifier)//移除函数
    {
        this.modifiers.Remove(modifier);
    }

}
