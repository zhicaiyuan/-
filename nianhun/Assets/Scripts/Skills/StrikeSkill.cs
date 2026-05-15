using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrikeSkill : Skill
{
    [SerializeField] private GameObject strikePerfab;
    [SerializeField] private UISkilltreeSlot strikeUnlockButton;
    public bool strikeUnlocked;

    protected override void Start()
    {
        base.Start();
        strikeUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockStrike);
    }

    private void UnlockStrike()
    {
        if (strikeUnlockButton.unlocked)
            strikeUnlocked = true;
    }

    public override bool CanSkill()
    {
        return strikeUnlocked && base.CanSkill();
    }

    public override bool Canuseskill()
    {
        return strikeUnlocked && base.Canuseskill();
    }

    public override void Useskill()
    {
        base.Useskill();
        AudioManager.instance.PlaySFX(25, null);
        GameObject newStrike = Instantiate(strikePerfab);
        newStrike.transform.SetParent(player.transform);
        newStrike.transform.localPosition = Vector3.zero;
        newStrike.transform.localRotation = Quaternion.identity;
        StartCoroutine(PlaySwordSound());
        Destroy(newStrike, 3.1f);
    }

    IEnumerator PlaySwordSound()
    {
        WaitForSeconds wait = new WaitForSeconds(2.2f);
        yield return wait;
        AudioManager.instance.PlaySFX(24, null);
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockStrike();
    }
}
