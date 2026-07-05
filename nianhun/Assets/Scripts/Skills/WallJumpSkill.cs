using UnityEngine;

public class WallJumpSkill : Skill
{
    private const string SkillTreeKey = "登墙跳";

    [SerializeField] private UISkilltreeSlot wallJumpUnlockButton;
    public bool wallJumpUnlocked;

    protected override void CheckUnlock()
    {
        wallJumpUnlocked = IsSlotUnlocked(wallJumpUnlockButton, SkillTreeKey);
    }

    public bool CanWallJump() => wallJumpUnlocked;
}
