using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkilltreeSlot : MonoBehaviour
{
    [SerializeField] private string skillname;
    [TextArea]
    [SerializeField] private string skilldescription;

    public bool unlocked;

    [SerializeField] private UISkilltreeSlot[] shouldBeUnlocked;
    [SerializeField] private UISkilltreeSlot[] shouldBelocked;

    [SerializeField] private Image skillimage;

    private void OnValidate()
    {
        gameObject.name = "技能槽UI -" + skillname;
    }


    private void Start()
    {
        skillimage = GetComponent<Image>();

        skillimage.color = Color.red;

        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());
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

        unlocked = true;
        skillimage.color = Color.green;
    }//解锁技能检测
}
