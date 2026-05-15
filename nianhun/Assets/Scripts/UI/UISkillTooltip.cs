using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillTooltip : MonoBehaviour
{
    public GameObject tooltip; // 提示框的 GameObject
    public TextMeshProUGUI tooltipText;   // 提示框中的文本组件
    [SerializeField] private RectTransform tooltipRectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false); // 初始隐藏提示框
            tooltipRectTransform = tooltip.GetComponent<RectTransform>();
            canvasGroup = tooltip.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts= false; // 忽略鼠标事件
               }
           }
       }

    void Update()
    {
        if (tooltip.activeSelf)
        {
            // 更新提示框位置为鼠标位置
            Vector2 mousePosition = Input.mousePosition;
            tooltipRectTransform.position = mousePosition;
        }
    }

    // 显示提示框
    public void ShowTooltip(string message)
    {
        if (tooltip != null && tooltipText != null)
        {
            tooltipText.text = message;

            // 获取鼠标位置并添加偏移量
            Vector2 mousePosition = Input.mousePosition;
            float offsetX = 100f; // X轴偏移量
            float offsetY = 100f; // Y轴偏移量
            tooltipRectTransform.position = new Vector2(mousePosition.x + offsetX, mousePosition.y + offsetY);

            tooltip.SetActive(true);
        }
    }

    // 隐藏提示框
    public void HideTooltip()
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
}
