using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeSkill : skill
{
    [SerializeField] private GameObject strikePerfab;
    public override bool CanSkill()
    {
        return base.CanSkill();
    }

    public override bool Canuseskill()
    {
        return base.Canuseskill();
    }

    public override void Useskill()
    {
        base.Useskill();
       
        GameObject newStrike = Instantiate(strikePerfab);
        newStrike.transform.SetParent(player.transform);
        newStrike.transform.localPosition = Vector3.zero;
        newStrike.transform.localRotation = Quaternion.identity;
        Destroy(newStrike,3.1f);
    }

    
}
