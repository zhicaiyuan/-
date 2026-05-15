using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParrySkill : Skill
{
    [Header("反击")]
    [SerializeField] private UISkilltreeSlot parryUnlockButton;
    public bool parryUnlocked;

    
    public override void Useskill()
    {
        base.Useskill();
    }
    protected override void Start()
    {
        base.Start();

        parryUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParry);

    }
    private void UnlockParry()
    {
        if(parryUnlockButton.unlocked)
            parryUnlocked = true;
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockParry();
    }
}
