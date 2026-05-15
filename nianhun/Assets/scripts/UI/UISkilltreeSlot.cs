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
        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());
        
    }
    private void Start()
    {
        ui = GetComponentInParent<UI>();
        skillimage = GetComponent<Image>();

        skillimage.color = lockedSkillColor;

        if(unlocked)
        {
            skillimage.color = Color.white;
        }
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
        skillimage.color = Color.white;
    }//解锁技能检测

    public void LoadData(GameData data)
    {
        if (data.skillTree.TryGetValue(skillname, out bool value))
        {
            Debug.Log($"Loading skill: {skillname}, unlocked: {value}");
            unlocked = value;
        }
        else
        {
            Debug.LogWarning($"Skill {skillname} not found in GameData.skillTree");
        }
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
