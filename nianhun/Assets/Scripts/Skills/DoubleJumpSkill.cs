using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleJumpSkill : Skill
{
    [SerializeField] private UISkilltreeSlot doubleJumpUnlockButton;
    public bool doubleJumpUnlocked;

    protected override void Start()
    {
        base.Start();
        doubleJumpUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockDoubleJump);
    }

    private void UnlockDoubleJump()
    {
        if (doubleJumpUnlockButton.unlocked)
            doubleJumpUnlocked = true;
    }

    public bool CanDoubleJump()
    {
        return doubleJumpUnlocked;
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockDoubleJump();
    }
}
