using UnityEngine;
using UnityEngine.UI;

public class UISkilltreeSlot : MonoBehaviour, ISaveManager
{
    private UI ui;
    private Image skillimage;

    [SerializeField] private int skillPrice;
    [SerializeField] private string skillname;
    public string SkillName => skillname;
    [TextArea]
    [SerializeField] private string skilldescription;
    [SerializeField] private Color lockedSkillColor;

    public bool unlocked;

    [SerializeField] private UISkilltreeSlot[] shouldBeUnlocked;
    [SerializeField] private UISkilltreeSlot[] shouldBelocked;

    private void OnValidate()
    {
        gameObject.name = "技能槽UI -" + skillname;
    }

    private void Awake()
    {
        skillimage = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(UnlockSkillSlot);
    }

    private void Start()
    {
        ui = GetComponentInParent<UI>();
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (skillimage == null)
            skillimage = GetComponent<Image>();

        if (skillimage != null)
            skillimage.color = unlocked ? Color.white : lockedSkillColor;
    }

    public void UnlockSkillSlot()
    {
        if (unlocked)
            return;

        if (shouldBeUnlocked != null)
        {
            for (int i = 0; i < shouldBeUnlocked.Length; i++)
            {
                if (shouldBeUnlocked[i] != null && shouldBeUnlocked[i].unlocked == false)
                {
                    Debug.Log("无法解锁技能：前置技能未学习");
                    return;
                }
            }
        }

        if (shouldBelocked != null)
        {
            for (int i = 0; i < shouldBelocked.Length; i++)
            {
                if (shouldBelocked[i] != null && shouldBelocked[i].unlocked)
                {
                    Debug.Log("无法解锁技能：互斥技能已学习");
                    return;
                }
            }
        }

        if (playermanger.instance == null || !playermanger.instance.HaveEnoughMoney(skillPrice))
            return;

        AudioManager.instance.PlaySFX(14, null);
        unlocked = true;
        RefreshVisual();

        if (SkillManager.instance != null)
            SkillManager.instance.RefreshAllSkillUnlocks();

        SaveManager.instance?.SaveGame();
    }

    public void LoadData(GameData data)
    {
        unlocked = data.skillTree != null &&
                   data.skillTree.TryGetValue(skillname, out bool value) &&
                   value;

        RefreshVisual();
    }

    public void SaveData(ref GameData data)
    {
        if (data.skillTree == null)
            data.skillTree = new SerializableDictionary<string, bool>();

        data.skillTree[skillname] = unlocked;
    }
}
