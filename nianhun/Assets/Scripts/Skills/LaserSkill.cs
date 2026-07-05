using UnityEngine;

public class LaserSkill : Skill
{
    private const string SkillTreeKey = "灵魂激流";

    [SerializeField] private GameObject laserperfab;
    [SerializeField] private UISkilltreeSlot laserUnlockButton;
    public bool laserUnlocked;

    protected override void CheckUnlock()
    {
        laserUnlocked = IsSlotUnlocked(laserUnlockButton, SkillTreeKey);
    }

    public override bool CanSkill() => laserUnlocked && base.CanSkill();

    public override bool Canuseskill() => laserUnlocked && base.Canuseskill();

    public override void Useskill()
    {
        AudioManager.instance.PlaySFX(26, null);
        GameObject newLaser = Instantiate(laserperfab);
        newLaser.transform.SetParent(player.transform);
        newLaser.transform.localPosition = Vector3.zero;
        newLaser.transform.localRotation = Quaternion.identity;
        StartCoroutine(LaserCoroutine());
        Destroy(newLaser, 2f);
        base.Useskill();
    }

    private System.Collections.IEnumerator LaserCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        AudioManager.instance.PlaySFX(27, null);
    }
}
