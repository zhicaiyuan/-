using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : CharaterStat
{
    private Player player;

    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
    }

    public override void Takedamdge(int _damage, bool iscrit)
    {
        base.Takedamdge(_damage, iscrit);
    }

    protected override void Die()
    {
        base.Die();

        player.Die();
    }

    public override void Decreasehealthby(int damage)
    {
        base.Decreasehealthby(damage);

        ItemDataEquipment currentArmor = Inventory.instance.GetEquipment(EquipmentType.护甲);//获取装备的护甲

        if (currentArmor != null)
            currentArmor.Effect(player.transform);
    }
}
