using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserSkill : Skill
{
    [SerializeField] private GameObject laserperfab;
    [SerializeField] private UISkilltreeSlot laserUnlockButton;
    public bool laserUnlocked;

    protected override void Start()
    {
        base.Start();
        laserUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockLaser);
    }

    private void UnlockLaser()
    {
        if (laserUnlockButton.unlocked)
            laserUnlocked = true;
    }

    public override bool CanSkill()
    {
        return laserUnlocked && base.CanSkill();
    }

    public override bool Canuseskill()
    {
        return laserUnlocked && base.Canuseskill();
    }

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

    IEnumerator LaserCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        AudioManager.instance.PlaySFX(27, null);
    }

    protected override void CheckUnlock()
    {
        UnlockLaser();
    }
}
