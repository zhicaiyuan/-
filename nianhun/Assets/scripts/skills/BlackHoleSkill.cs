using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackHoleSkill : Skill
{
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
    public bool useSkill = false;

    BlackholeSkillcontroller currentBlackhole;
    public override bool Canuseskill()
    {
        return blackHoleUnlocked && base.Canuseskill();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void Useskill()
    {
        base.Useskill();
        useSkill = true;
        GameObject newBlackHole = Instantiate(blackHolePerfab, player.transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX(20, null);
        currentBlackhole = newBlackHole.GetComponent<BlackholeSkillcontroller>();

        currentBlackhole.SetUpBlackHole(maxSize, growSpeed, shrinkSpeed, amountOfAttacks, cloneCooldown, blackHoleDuration);
    }

    protected override void Update()
    {
        base.Update();
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

    public override bool CanSkill()
    {
        return base.CanSkill();
    }

    protected override void Start()
    {
        base.Start();
        blackHoleUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockBlackHole);
    }

    private void UnlockBlackHole()
    {
        if (blackHoleUnlockButton.unlocked)
            blackHoleUnlocked = true;
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockBlackHole();
    }
}