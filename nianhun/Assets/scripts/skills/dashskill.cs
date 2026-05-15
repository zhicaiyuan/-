using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dashskill : Skill
{
    [HideInInspector]public bool dashUnlocked;
    [SerializeField] private UISkilltreeSlot dashUnlockButton;
    public override void Useskill()
    {
        base.Useskill(); 

    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockDash();
    }

    protected override void Start()
    {
        base.Start();
        dashUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockDash);
    }
    
    
    private void UnlockDash()
    {
        if(dashUnlockButton.unlocked) 
            dashUnlocked = true;
    }

    
}
