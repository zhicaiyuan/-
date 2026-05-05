using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill : MonoBehaviour
{
    public float cooldown;
    protected float cooldowntime;

    protected Player player;

    protected virtual void Update()
    {
        player = playermanger.instance.player;
        cooldowntime -= Time.deltaTime;
    }

    public virtual bool Canuseskill()
    {
        if(cooldowntime < 0)
        {
            Useskill();
            cooldowntime = cooldown;
            return true;
        }

        player.fx.CreatePopUpText("技能冷却中");
        return false;
    }


    public virtual void Useskill()
    {

    }
}
