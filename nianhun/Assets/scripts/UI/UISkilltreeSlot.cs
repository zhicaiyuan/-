using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISkilltreeSlot : MonoBehaviour ,ISaveManager
{
    private UI ui;
    private Image skillimage;

    [SerializeField] private int skillPrice;
    [SerializeField] private string skillname;
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
        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());
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

        skillimage.color = unlocked ? Color.white : lockedSkillColor;
    }

    public void UnlockSkillSlot()
    {
        for (int i = 0; i < shouldBeUnlocked.Length; i++)
        {
            if (shouldBeUnlocked[i].unlocked == false)
            {
                Debug.Log("无法解锁技能");
                return;
            }
        }//前置条件

        for (int i = 0; i < shouldBelocked.Length; i++)
        {
            if (shouldBelocked[i].unlocked == true)
            {               
                    Debug.Log("无法解锁技能");
                    return;
            }
        }//同位约束

        if (playermanger.instance.HaveEnoughMoney(skillPrice) == false)
            return;
        AudioManager.instance.PlaySFX(14, null);
        unlocked = true;
        RefreshVisual();
    }//解锁技能检测

    public void LoadData(GameData data)
    {
        if (data.skillTree.TryGetValue(skillname, out bool value))
            unlocked = value;
        else
            unlocked = false;

        RefreshVisual();
    }

    public void SaveData(ref GameData data)
    {
        if(data.skillTree.TryGetValue(skillname, out bool value))
        {
            data.skillTree[skillname] = unlocked;
        }
        else
        {
            data.skillTree.Add(skillname, unlocked);
        }
    }
}
