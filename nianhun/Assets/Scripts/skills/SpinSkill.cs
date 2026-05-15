using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinSkill : Skill
{
    public float spinDuration = 3;
    [SerializeField] private GameObject spinPerfab;
    [SerializeField] private UISkilltreeSlot spinUnlockButton;
    public bool spinUnlocked;

    protected override void Start()
    {
        base.Start();
        spinUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSpin);
    }

    private void UnlockSpin()
    {
        if (spinUnlockButton.unlocked)
            spinUnlocked = true;
    }

    public override bool CanSkill()
    {
        return spinUnlocked && base.CanSkill();
    }

    public override bool Canuseskill()
    {
        return spinUnlocked && base.Canuseskill();
    }

    public override void Useskill()
    {
        base.Useskill();
        GameObject newSpin = Instantiate(spinPerfab, player.transform.position, Quaternion.identity);
        newSpin.transform.SetParent(player.transform);
        AudioManager.instance.PlaySFX(23, null);
        Destroy(newSpin, spinDuration);
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockSpin();
    }
}
