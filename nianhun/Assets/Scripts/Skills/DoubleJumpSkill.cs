using UnityEngine;

public class DoubleJumpSkill : Skill
{
    private const string SkillTreeKey = "二段跳";

    [SerializeField] private UISkilltreeSlot doubleJumpUnlockButton;
    public bool doubleJumpUnlocked;

    protected override void CheckUnlock()
    {
        doubleJumpUnlocked = IsSlotUnlocked(doubleJumpUnlockButton, SkillTreeKey);
    }

    public bool CanDoubleJump() => doubleJumpUnlocked;
}
