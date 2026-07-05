using UnityEngine;

public class Dashskill : Skill
{
    private const string SkillTreeKey = "冲刺";

    [HideInInspector] public bool dashUnlocked;
    [SerializeField] private UISkilltreeSlot dashUnlockButton;

    protected override void CheckUnlock()
    {
        dashUnlocked = IsSlotUnlocked(dashUnlockButton, SkillTreeKey);
    }

    public override void Useskill()
    {
        base.Useskill();
    }
}
