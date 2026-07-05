using System.Collections;
using UnityEngine;

public class StrikeSkill : Skill
{
    private const string SkillTreeKey = "剑魂一击";

    [SerializeField] private GameObject strikePerfab;
    [SerializeField] private UISkilltreeSlot strikeUnlockButton;
    public bool strikeUnlocked;

    protected override void CheckUnlock()
    {
        strikeUnlocked = IsSlotUnlocked(strikeUnlockButton, SkillTreeKey);
    }

    public override bool CanSkill() => strikeUnlocked && base.CanSkill();

    public override bool Canuseskill() => strikeUnlocked && base.Canuseskill();

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

    private IEnumerator PlaySwordSound()
    {
        yield return new WaitForSeconds(2.2f);
        AudioManager.instance.PlaySFX(24, null);
    }
}
