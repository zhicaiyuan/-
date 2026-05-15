using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallJumpSkill : Skill
{
    [SerializeField] private UISkilltreeSlot wallJumpUnlockButton;
    public bool wallJumpUnlocked;

    protected override void Start()
    {
        base.Start();
        wallJumpUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockWallJump);
    }

    private void UnlockWallJump()
    {
        if (wallJumpUnlockButton.unlocked)
            wallJumpUnlocked = true;
    }

    public bool CanWallJump()
    {
        return wallJumpUnlocked;
    }
    void Update()
    {
        
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockWallJump();
    }

    public override bool CanSkill()
    {
        return base.CanSkill();
    }
}
