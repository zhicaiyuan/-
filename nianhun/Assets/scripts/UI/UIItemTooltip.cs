using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIItemTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemnametext;
    [SerializeField] private TextMeshProUGUI itemtypetext;
    [SerializeField] private TextMeshProUGUI itemdescription;

    [SerializeField] private RectTransform _tooltipRect;
    [SerializeField] private Canvas _parentCanvas;

    [SerializeField] private Vector2 mouseOffset = new Vector2(15, -15);//鼠标偏移设置
    [SerializeField] private float screenPadding = 10f;//离屏幕最小距离

    [SerializeField] private int defaultFontSize = 40;
    private void Start()
    {
        
    }
    private void Update()
    {
        if(!gameObject.activeSelf)
            return;
        FollowMousePosition();
    }
    public void ShowTooltip(ItemDataEquipment item)
    {
        if(item == null) 
            return;

        itemnametext.text = item.itemname;
        itemtypetext.text = item.equipmenttype.ToString();
        itemdescription.text = item.GetDescription();

        if (itemnametext.text.Length > 6)
            itemnametext.fontSize = itemnametext.fontSize * .7f;
        else
            itemnametext.fontSize = defaultFontSize;

        gameObject.SetActive(true);

    }//展示提示框
    public void HideTooltip()
    {
        itemnametext.fontSize = defaultFontSize;
        gameObject.SetActive(false);
    }//隐藏
    private void FollowMousePosition()
    {
        Vector2 mouseScreenPos = Input.mousePosition;//获取鼠标位置

        // 1. 屏幕坐标 → UI局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            mouseScreenPos,
            _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera,
            out Vector2 localPos
        );

        // 2. 加上鼠标偏移
        localPos += mouseOffset;

        // 3. 边界保护：防止提示框飞出屏幕
        Vector2 tooltipSize = _tooltipRect.rect.size;
        Rect canvasRect = (_parentCanvas.transform as RectTransform).rect;

        // 左右边界
        localPos.x = Mathf.Clamp(localPos.x, canvasRect.xMin + screenPadding, canvasRect.xMax - tooltipSize.x - screenPadding);
        // 上下边界
        localPos.y = Mathf.Clamp(localPos.y, canvasRect.yMin + tooltipSize.y + screenPadding, canvasRect.yMax - screenPadding);

        // 4. 赋值位置
        _tooltipRect.anchoredPosition = localPos;
    }

}
