using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public float cooldown;
    [HideInInspector]public bool usedskill = false;
    protected float cooldowntime;

    protected Player player;

    protected virtual void Update()
    {
        cooldowntime -= Time.deltaTime;
    }

    protected virtual void Start()
    {
        player = playermanger.instance.player;
        CheckUnlock();
    }

    protected virtual void CheckUnlock()
    {
        
    }

    public virtual bool Canuseskill()
    {
        usedskill = true;
        if(cooldowntime <= 0)
        {
            Useskill();
            cooldowntime = cooldown;
            return true;
        }

        player.fx.CreatePopUpText("技能冷却中");
        return false;
    }
    public virtual bool CanSkill()
    {
        if (cooldowntime <= 0)
            return true;
        else
            return false;
    }

    public virtual void Useskill()
    {
        usedskill = true;
    }
}
