using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICraftwindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemname;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button craftbutton;
        
    [SerializeField] private Image[] materialImage;

    public void SetupCraftWindow(ItemDataEquipment data)
    {
        craftbutton.onClick.RemoveAllListeners();

        for (int i = 0; i < materialImage.Length; i++)
        {
            materialImage[i].color = Color.clear;
            materialImage[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.clear;//清除图像
        }
        for (int i = 0; i < data.craftingMaterials.Count; i++)
        {
            if (data.craftingMaterials.Count > materialImage.Length)
            { 
                
                Debug.Log("需求供应数过多"); 
                return;
            }

            materialImage[i].sprite = data.craftingMaterials[i].data.icon;
            materialImage[i].color = Color.white;

            TextMeshProUGUI materialSlotText = materialImage[i].GetComponentInChildren<TextMeshProUGUI>();
            materialSlotText.color = Color.white;
            materialSlotText.text = data.craftingMaterials[i].stackSize.ToString();//加载材料图像和文字
        }

        itemIcon.sprite = data.icon;
        itemname.text = data.itemname;
        itemDescription.text = data.GetDescription();//加载制造的物品的图像和名字

        craftbutton.onClick.AddListener(() => Inventory.instance.CanCraft(data, data.craftingMaterials));
    }
}
