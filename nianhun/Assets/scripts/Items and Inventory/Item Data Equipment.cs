
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum EquipmentType
{
    武器,
    护甲,
    饰品,
    道具
}
[CreateAssetMenu(fileName = "新物品信息", menuName = "装备")]//类型名字
public class ItemDataEquipment : ItemData
{
    public EquipmentType equipmenttype;

    public float itemCooldown;
    public ItemEffect[] itemEffects;

    [Header("主属性增幅")]
    public int strength;
    public int agility;
    public int intellgence;
    public int vitality;

    [Header("输出属性")]
    public int damage;
    public int critchance;
    public int critpower;

    [Header("防御属性")]
    public int health;
    public int armor;
    public int evasion;
    public int magicresistance;

    [Header("魔法属性")]
    public int firedamage;
    public int icedamage;
    public int _lightingdamage;

    [Header("工艺需求")]
    public List<InventoryItem> craftingMaterials;

    private int minDescriptioLength;


    public void Effect(Transform enemyposition)
    {
        foreach (var item in itemEffects)//对应每个有效果的物品
        {
            item.ExecuteEffect(enemyposition);//执行效果
        }
    }

    public void AddModifiers()
    {
        PlayerStat playerstat = playermanger.instance.player.GetComponent<PlayerStat>();//获取palyerstat组件

        playerstat.strength.Addmodifier(strength);
        playerstat.agility.Addmodifier(agility);
        playerstat.intelgence.Addmodifier(intellgence);
        playerstat.vitality.Addmodifier(vitality);

        playerstat.damage.Addmodifier(damage);
        playerstat.critchance.Addmodifier(critchance);
        playerstat.critdamage.Addmodifier(critpower);

        playerstat.maxhp.Addmodifier(health);
        playerstat.armor.Addmodifier(armor);
        playerstat.evasion.Addmodifier(evasion);
        playerstat.magicresistance.Addmodifier(magicresistance);

        playerstat.firedamage.Addmodifier(firedamage);
        playerstat.icedamage.Addmodifier(icedamage);
        playerstat._lightingdamage.Addmodifier(_lightingdamage);//附加值


    }

    public void RemoveModifiers() 
    {
        PlayerStat playerstat = playermanger.instance.player.GetComponent<PlayerStat>();//同理

        playerstat.strength.Removemodifier(strength);
        playerstat.agility.Removemodifier(agility);
        playerstat.intelgence.Removemodifier(intellgence);
        playerstat.vitality.Removemodifier(vitality);

        playerstat.damage.Removemodifier(damage);
        playerstat.critchance.Removemodifier(critchance);
        playerstat.critdamage.Removemodifier(critpower);

        playerstat.maxhp.Removemodifier(health);
        playerstat.armor.Removemodifier(armor);
        playerstat.evasion.Removemodifier(evasion);
        playerstat.magicresistance.Removemodifier(magicresistance);

        playerstat.firedamage.Removemodifier(firedamage);
        playerstat.icedamage.Removemodifier(icedamage);
        playerstat._lightingdamage.Removemodifier(_lightingdamage);
    }

    public override string GetDescription()
    {
        sb.Length = 0;
        magicresistance = 0;

        AddItemDescription(strength, "力量");
        AddItemDescription(intellgence, "智力");
        AddItemDescription(agility, "敏捷");
        AddItemDescription(vitality, "体质");

        AddItemDescription(damage, "伤害");
        AddItemDescription(critchance, "暴击率");
        AddItemDescription(critpower, "暴击倍率");

        AddItemDescription(health, "最大血量");
        AddItemDescription(evasion, "闪避");
        AddItemDescription(armor, "防御");
        AddItemDescription(magicresistance, "魔抗");

        AddItemDescription(firedamage, "火焰伤害");
        AddItemDescription(icedamage, "寒冰伤害");
        AddItemDescription(_lightingdamage, "雷电伤害");

        if(minDescriptioLength < 5)
        {
            for (int i = 0; i < 5 - minDescriptioLength; i++)
            {
                sb.AppendLine();
                sb.Append("");
            }
        }


        return sb.ToString();
    }

    private void AddItemDescription(int value,string name)
    {
        if(value != 0)
        {
            if(sb.Length > 0)
                sb.AppendLine();
            if(value > 0)
                sb.Append("+" + value +" " + name);

            minDescriptioLength++;
        }
    }
}
