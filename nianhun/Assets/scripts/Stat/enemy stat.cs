using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemystat : CharaterStat
{
    private Enemy enemy;
    private ItemDrop myDropSystem;

    [Header("等级信息")]
    [SerializeField] private int level = 1;

    [Range(0f, 1f)]
    [SerializeField] private float percantageModifier = 0.1f;//每级增加百分比

    protected override void Start()
    {
        ApplyLevelModifiers();
        base.Start();
        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();

    }

    private void ApplyLevelModifiers()
    {
        Modify(damage);
        Modify(critchance);
        Modify(critdamage);

        Modify(maxhp);
        Modify(armor);
        Modify(evasion);
        Modify(magicresistance);

        Modify(strength);
        Modify(agility);
        Modify(intelgence);
        Modify(vitality);

        Modify(icedamage);
        Modify(firedamage);
        Modify(_lightingdamage);
    }//增加数值

    private void Modify(Stat stat)
    {
        for(int i = 1; i < level; i++)//循环倍乘
        {
            float modifer = stat.Getvalue() * percantageModifier;//每级增强

            stat.Addmodifier(Mathf.RoundToInt(modifer));
        }
    }

    public override void Takedamdge(int _damage)
    {
        DamageNumberPool.instance.SpawnDamageNumber(transform.position, _damage, cancrit());
        base.Takedamdge(_damage);

        
    }
    protected override void Die()
    {

        if (!enemy.isDead) 
        {
        enemy.isDead = true;
        enemy.Die();
        myDropSystem.GenerateDrop();

        }
    }
    
}
