using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;

    public Dashskill dash { get; private set; }
    public CloneSkill clone { get; private set; }
    public BlackHoleSkill blackhole { get; private set; }
    public SpinSkill spin { get; private set; }
    public StrikeSkill strike { get; private set; }
    public LaserSkill laser { get; private set; }
    public ParrySkill parry { get; private set; }
    public DoubleJumpSkill doubleJump { get; private set; }
    public WallJumpSkill wallJump { get; private set; }

    private Dictionary<string, UISkilltreeSlot> slotByName;

    public void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance.gameObject);

        instance = this;
    }

    private void OnEnable()
    {
        InvalidateSlotCache();
    }

    private void Start()
    {
        dash = GetComponent<Dashskill>();
        clone = GetComponent<CloneSkill>();
        blackhole = GetComponent<BlackHoleSkill>();
        spin = GetComponent<SpinSkill>();
        strike = GetComponent<StrikeSkill>();
        laser = GetComponent<LaserSkill>();
        parry = GetComponent<ParrySkill>();
        doubleJump = GetComponent<DoubleJumpSkill>();
        wallJump = GetComponent<WallJumpSkill>();

        RefreshAllSkillUnlocks();
    }

    public void InvalidateSlotCache()
    {
        slotByName = null;
    }

    public void RefreshAllSkillUnlocks()
    {
        InvalidateSlotCache();
        EnsureSlotCache();

        dash?.RefreshUnlock();
        blackhole?.RefreshUnlock();
        spin?.RefreshUnlock();
        strike?.RefreshUnlock();
        laser?.RefreshUnlock();
        parry?.RefreshUnlock();
        doubleJump?.RefreshUnlock();
        wallJump?.RefreshUnlock();
    }

    public bool IsSkillUnlocked(string skillName, UISkilltreeSlot slotRef = null)
    {
        EnsureSlotCache();

        if (slotRef != null && slotRef.unlocked)
            return true;

        if (!string.IsNullOrEmpty(skillName) &&
            slotByName != null &&
            slotByName.TryGetValue(skillName, out UISkilltreeSlot sceneSlot) &&
            sceneSlot != null &&
            sceneSlot.unlocked)
            return true;

        return SaveManager.instance != null && SaveManager.instance.IsSkillUnlocked(skillName);
    }

    private void EnsureSlotCache()
    {
        if (slotByName != null)
            return;

        slotByName = new Dictionary<string, UISkilltreeSlot>();

        foreach (UISkilltreeSlot slot in Object.FindObjectsOfType<UISkilltreeSlot>(includeInactive: true))
        {
            if (slot == null || string.IsNullOrEmpty(slot.SkillName))
                continue;

            slotByName[slot.SkillName] = slot;
        }
    }
}
