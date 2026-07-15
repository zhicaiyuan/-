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

    [Header("解锁所需道具（可选）")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private int requiredItemAmount = 1;

    public bool unlocked;

    [SerializeField] private UISkilltreeSlot[] shouldBeUnlocked;
    [SerializeField] private UISkilltreeSlot[] shouldBelocked;

    private void OnValidate()
    {
        gameObject.name = "技能槽UI -" + skillname;
        if (requiredItemAmount < 1)
            requiredItemAmount = 1;
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
                    ShowUnlockFail("前置技能未学习");
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
                    ShowUnlockFail("互斥技能已学习");
                    return;
                }
            }
        }

        if (playermanger.instance == null)
            return;

        if (playermanger.instance.CurrentCurrencyAmount() < skillPrice)
        {
            ShowUnlockFail("金钱不足");
            return;
        }

        if (!HasRequiredItem())
            return;

        // 先扣道具再扣钱，避免半成功
        if (!ConsumeRequiredItem())
            return;

        if (!playermanger.instance.HaveEnoughMoney(skillPrice))
        {
            // 理论上不会到这里；若发生则尝试不回滚道具，只提示
            ShowUnlockFail("金钱不足");
            return;
        }

        AudioManager.instance.PlaySFX(14, null);
        unlocked = true;
        RefreshVisual();

        if (SkillManager.instance != null)
            SkillManager.instance.RefreshAllSkillUnlocks();

        SaveManager.instance?.SaveGame();
    }

    private bool HasRequiredItem()
    {
        if (requiredItem == null || requiredItemAmount <= 0)
            return true;

        if (Inventory.instance == null)
        {
            ShowUnlockFail("背包不可用");
            return false;
        }

        int have = Inventory.instance.GetItemCount(requiredItem);
        if (have >= requiredItemAmount)
            return true;

        string itemName = string.IsNullOrEmpty(requiredItem.itemname) ? requiredItem.name : requiredItem.itemname;
        ShowUnlockFail($"需要 {itemName} x{requiredItemAmount}");
        Debug.Log($"无法解锁技能：缺少道具 {itemName}（{have}/{requiredItemAmount}）");
        return false;
    }

    private bool ConsumeRequiredItem()
    {
        if (requiredItem == null || requiredItemAmount <= 0)
            return true;

        if (Inventory.instance != null &&
            Inventory.instance.TryConsumeItem(requiredItem, requiredItemAmount))
            return true;

        ShowUnlockFail("道具不足");
        return false;
    }

    private static void ShowUnlockFail(string message)
    {
        if (playermanger.instance != null &&
            playermanger.instance.player != null &&
            playermanger.instance.player.fx != null)
        {
            playermanger.instance.player.fx.CreatePopUpText(message);
        }
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
