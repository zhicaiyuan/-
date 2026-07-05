using UnityEngine;

public class ParrySkill : Skill
{
    private const string SkillTreeKey = "反击";

    [SerializeField] private UISkilltreeSlot parryUnlockButton;
    public bool parryUnlocked;

    protected override void CheckUnlock()
    {
        parryUnlocked = IsSlotUnlocked(parryUnlockButton, SkillTreeKey);
    }

    public override void Useskill()
    {
        base.Useskill();
    }
}
