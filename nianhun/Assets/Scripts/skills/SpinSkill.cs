using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinSkill : skill
{
    public float spinDuration = 3;
    [SerializeField] private GameObject spinPerfab;
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
        GameObject newSpin = Instantiate(spinPerfab, player.transform.position, Quaternion.identity);
        newSpin.transform.SetParent(player.transform);
        AudioManager.instance.PlaySFX(23, null);
        Destroy(newSpin,spinDuration);
    }

}
