using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : CharaterStat
{
    private Player player;
    public bool isdead = false;
    private bool currencysave = false;

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
        if (isdead)
            return;

        isdead = true;

        if (!currencysave)
        {
            GameManager.instance.lostCurrencyAmount = playermanger.instance.currency;
            playermanger.instance.currency = 0;
            currencysave = true;
        }

        if (GameManager.instance.lostCurrencyAmount > 0)
            GameManager.instance.DropLostCurrencyCorpse(player.transform.position, player);

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
