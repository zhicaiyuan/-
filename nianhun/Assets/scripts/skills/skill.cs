using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill : MonoBehaviour
{
    public float cooldown;
    protected float cooldowntime;

    protected virtual void Update()
    {
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

        return false;
    }


    public virtual void Useskill()
    {

    }
}
