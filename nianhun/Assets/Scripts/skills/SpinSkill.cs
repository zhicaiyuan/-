using UnityEngine;

public class SpinSkill : Skill
{
    private const string SkillTreeKey = "灵魂风暴";

    [Header("Spin 参数")]
    [Tooltip("技能持续时长（秒），同时控制玩家 Spin 状态与特效销毁")]
    public float spinDuration = 3;
    [Tooltip("对范围内敌人造成伤害的间隔（秒）")]
    [SerializeField] private float attackInterval = 0.8f;

    [SerializeField] private GameObject spinPerfab;
    [SerializeField] private UISkilltreeSlot spinUnlockButton;
    public bool spinUnlocked;

    protected override void CheckUnlock()
    {
        spinUnlocked = IsSlotUnlocked(spinUnlockButton, SkillTreeKey);
    }

    public override bool CanSkill() => spinUnlocked && base.CanSkill();

    public override bool Canuseskill() => spinUnlocked && base.Canuseskill();

    public override void Useskill()
    {
        base.Useskill();
        GameObject newSpin = Instantiate(spinPerfab, player.transform.position, Quaternion.identity);
        newSpin.transform.SetParent(player.transform);

        if (newSpin.TryGetComponent(out SpinSkillController controller))
            controller.SetAttackInterval(attackInterval);

        AudioManager.instance.PlaySFX(23, null);
        Destroy(newSpin, spinDuration);
    }
}
