using UnityEngine;

public class BlackHoleSkill : Skill
{
    private const string SkillTreeKey = "灵魂湮灭";

    [SerializeField] private float cloneCooldown;
    [SerializeField] private int amountOfAttacks;
    [SerializeField] private float blackHoleDuration;
    [Space]
    [SerializeField] private GameObject blackHolePerfab;
    [SerializeField] private float maxSize;
    [SerializeField] private float growSpeed;
    [SerializeField] private float shrinkSpeed;
    [SerializeField] private UISkilltreeSlot blackHoleUnlockButton;
    public bool blackHoleUnlocked;
    public bool useSkill;

    BlackholeSkillcontroller currentBlackhole;

    protected override void CheckUnlock()
    {
        blackHoleUnlocked = IsSlotUnlocked(blackHoleUnlockButton, SkillTreeKey);
    }

    public override bool Canuseskill() => blackHoleUnlocked && base.Canuseskill();

    public override void Useskill()
    {
        base.Useskill();
        useSkill = true;
        GameObject newBlackHole = Instantiate(blackHolePerfab, player.transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX(20, null);
        currentBlackhole = newBlackHole.GetComponent<BlackholeSkillcontroller>();
        currentBlackhole.SetUpBlackHole(maxSize, growSpeed, shrinkSpeed, amountOfAttacks, cloneCooldown, blackHoleDuration);
    }

    public bool BlackholeFinish()
    {
        useSkill = false;
        if (!currentBlackhole)
            return false;

        if (currentBlackhole.playerCanExitState)
        {
            currentBlackhole = null;
            return true;
        }

        return false;
    }

    public override bool CanSkill() => base.CanSkill();
}
