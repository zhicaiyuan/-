
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
public enum StatType
{
    strength,
    agility,
    intelegence,
    vitality,
    damage,
    critchance,
    critpower,
    health,
    armor,
    evasion,
    magicres,
    firedamage,
    icedamage,
    lightingdamage
}//枚举类型所有可能的状态
public class CharaterStat : MonoBehaviour
{
    private EntityFx fx;

    #region 属性
    [Header("主属性")]
    public Stat strength;//力量提升攻击强度暴击倍率
    public Stat agility;//敏捷提升闪避率暴击率
    public Stat intelgence;//智力提升魔法伤害
    public Stat vitality;//体质提升血量与防御


    [Header("防御属性")]
    public Stat armor;//护甲
    public Stat maxhp;//血量
    public Stat evasion;//闪避
    public Stat magicresistance;//法抗

    [Header("输出属性")]
    public Stat damage;//基础伤害
    public Stat critchance;//暴击率
    public Stat critdamage;//暴击伤害 默认150

    [Header("属性伤害")]
    public Stat firedamage;//火伤
    public Stat icedamage;//冰伤
    public Stat _lightingdamage;//电伤

    public bool isfired;//持续造成伤害
    public bool ischilled;//虚弱
    public bool isshocked;//减护甲与魔抗
    #endregion

    private float firedamagecooldawn = 1;//间隔
    private float firedamagetimer;//时间间隔计算
    private int firingdamage;//燃烧伤害

    

    private float firetimer;//燃烧持续时间
    private float chilledtime;//寒冰持续时间
    private float shockedtime;//易伤持续时间


    public int currenthealth;

    public System.Action onhealthchanged;

    protected virtual void Start()
    {
        //初始化数值
        critdamage.setdefaultvalue(150);
        currenthealth = Getmaxhealthvalue();
        fx =GetComponent<EntityFx>();//获取fx

    }

    protected virtual void Update()
    {
        firetimer -=Time.deltaTime;
        chilledtime -= Time.deltaTime;
        shockedtime -= Time.deltaTime;

        firedamagetimer -=Time.deltaTime;

        if(firetimer < 0)
        {
            isfired = false;
        }

        if(shockedtime < 0)
        {
            isshocked = false;
        }

        if(chilledtime  < 0)
        {
            ischilled = false;
        }
        if(firedamagetimer < 0 && isfired)
        {
            Debug.Log("燃烧中！"+ firingdamage);

            Decreasehealthby(firingdamage);

            if(currenthealth <= 0)
            {
                Die();
            }
            firedamagetimer = firedamagecooldawn;
        }
    }

    public virtual void IncreaseStatBy(int modifier,float duration,Stat statToModify)
    {
        StartCoroutine(StatModCorotuien(modifier, duration, statToModify));
    }

    private IEnumerator StatModCorotuien(int modifier, float duration,Stat statToModify)
    {
        statToModify.Addmodifier(modifier);

        yield return new WaitForSeconds(duration);

        statToModify.Removemodifier(modifier);
    }//设置添加buff的协程

    public virtual void Decreasehealthby(int damage)
    {
        currenthealth -= damage;

        if(onhealthchanged != null)
        {
            onhealthchanged();
        }
    }//生命减少时候触发时间整理

    public virtual void Dodamage(CharaterStat targetstat)//伤害函数
    {
        bool flowControl = canavoidattack(targetstat);
        if (flowControl)//判断闪避
        {
            return;
        }

        int totaldamage = damage.Getvalue() + strength.Getvalue();//计算伤害
        if (cancrit())//判断暴击
        {
            Debug.Log("暴击！");
            totaldamage=calculatecritaldamage(totaldamage);
        }
        if (ischilled)//寒冰效果
        {
            totaldamage =Mathf.RoundToInt(totaldamage * 0.8f);
        }
        totaldamage = checkarmor(targetstat, totaldamage);//减护甲
        targetstat.Takedamdge(totaldamage);
        Domagicdamage(targetstat);
        
    }

    private static int checkarmor(CharaterStat targetstat, int totaldamage)
    {
        if (targetstat.isshocked)//雷电效果
        {
            totaldamage -= Mathf.RoundToInt(targetstat.armor.Getvalue() * 0.7f);
        }
        else
        {
        totaldamage -= targetstat.armor.Getvalue();
        }
        totaldamage = Mathf.Clamp(totaldamage, 0, int.MaxValue);//防止伤害为负而治疗
        return totaldamage;
    }//护甲函数

    private static bool canavoidattack(CharaterStat targetstat)
    {
        int totalevasion = targetstat.evasion.Getvalue() + targetstat.strength.Getvalue();//计算闪避

        if (UnityEngine.Random.Range(0, 100) < totalevasion)
        {
            Debug.Log("成功闪避");
            return true;
        }

        return false;
    }//闪避函数

    public virtual void Takedamdge(int _damage)//受伤函数
    {
        Decreasehealthby(_damage);

        if (currenthealth <= 0)
        {
            Die();
        }
    }

    public virtual void IncreaseHealthBy(int amount)
    {
        currenthealth += amount;

        if(currenthealth > Getmaxhealthvalue())
        {
            currenthealth = Getmaxhealthvalue();
        }//如果超出最大变为最大

        if(onhealthchanged != null)
        {
            onhealthchanged();
        }//更新血条
    }//加血函数


    protected virtual void Die()
    {
        
    }//死亡函数

    private bool cancrit()
    {
        int totalcritcalchance = critchance.Getvalue() + agility.Getvalue();//计算暴击

        if(UnityEngine.Random.Range(0,100) <= totalcritcalchance)//暴击判断
        {
            return true;
        }

        return false;
    }//暴击函数

    private int calculatecritaldamage(int damage)
    {
        float totalcritdamage = (critdamage.Getvalue() + agility.Getvalue()) * 0.01f;//
        float critpower = damage * totalcritdamage;
        return Mathf.RoundToInt(critpower);//返回整数
    }//暴伤函数

    public virtual void Domagicdamage(CharaterStat Targetstats)//魔法伤害函数
    {
        int _firedamage = firedamage.Getvalue();
        int _icedamage = icedamage.Getvalue();
        int _lightdamage = _lightingdamage.Getvalue();//获取各类元素伤害

        int totalmagicdamage = _firedamage + _icedamage + _lightdamage + intelgence.Getvalue();
        totalmagicdamage -= Targetstats.magicresistance.Getvalue() +(Targetstats.intelgence.Getvalue());//减抗性
        totalmagicdamage = Mathf.Clamp(totalmagicdamage,0, int.MaxValue);//取整

        Targetstats.Takedamdge(totalmagicdamage);

        if(Mathf.Max(_firedamage,_icedamage,_lightdamage) <= 0)//debug伤害为0而附加
        {
            return;
        }

        bool canapplyfire = _firedamage > _icedamage && _firedamage > _lightdamage;
        bool canapplychill = _icedamage > _firedamage && _icedamage > _lightdamage;
        bool canapplyshock = _lightdamage > _firedamage && _lightdamage > _icedamage;//判断元素

        
            if(UnityEngine.Random.value < .5f && _firedamage > 0)
            {
                canapplyfire = true;
                Targetstats.ApplyAilments(canapplyfire, canapplychill, canapplyshock);
                Debug.Log("fire");
                return;
            }//若值相等随机附加元素

            if(UnityEngine.Random.value < .5f && _icedamage > 0)
            {
                canapplychill = true;
                Targetstats.ApplyAilments(canapplyfire, canapplychill, canapplyshock);
                Debug.Log("ice");
                return;
            }
            
            if( UnityEngine.Random.value < .5f && _lightdamage > 0)
            {
                canapplyshock  =  true;
                Targetstats.ApplyAilments(canapplyfire, canapplychill, canapplyshock);
                Debug.Log("lingt");
                return;
            }
        
        if (canapplyfire)
        {
            Targetstats.Setupfiringdamage(Mathf.RoundToInt(_firedamage * .1f));//持续燃烧
        }

        Targetstats.ApplyAilments(canapplyfire,canapplychill,canapplyshock);
        
    }

    public void Setupfiringdamage(int _damage)
    {
        firingdamage = _damage;
    }//设置火伤

    public void ApplyAilments(bool fire,bool chill,bool shock)//施加元素效果
    {
        if(ischilled || isfired || isshocked)
        {
            return;
        }
        if (fire)
        {
            isfired = fire;
            firetimer = 5;

            fx.InvokeFireFxFor(5);
        }
        if (chill)
        {
            ischilled = chill;
            chilledtime = 8;

            GetComponent<Entity>().SlowEntityBy(.2f, 8);//.2f为减速百分比 8为时间
            fx.InvokeChillFxFor(8);
        }
        if (shock)
        {
            isshocked = shock;
            shockedtime = 3;
            fx.InvokeShockFxFor(3);
        }//各元素持续时间
    }

    public int Getmaxhealthvalue()
    {
        return maxhp.Getvalue() + vitality.Getvalue() * 5;
    }//获取生命值

    public Stat GetStat(StatType stattype)
    {
        if (stattype == StatType.strength) return strength;
        else if (stattype == StatType.agility) return agility;
        else if (stattype == StatType.intelegence) return intelgence;
        else if (stattype == StatType.armor) return armor;
        else if (stattype == StatType.evasion) return evasion;
        else if (stattype == StatType.magicres) return magicresistance;
        else if (stattype == StatType.vitality) return vitality;
        else if (stattype == StatType.damage) return damage;
        else if (stattype == StatType.critchance) return critchance;
        else if (stattype == StatType.critpower) return critdamage;
        else if (stattype == StatType.health) return maxhp;
        else if (stattype == StatType.firedamage) return firedamage;
        else if (stattype == StatType.icedamage) return icedamage;
        else if (stattype == StatType.lightingdamage) return _lightingdamage;
        else return null;


    }//通过枚举找到对应stat类的数据
}
