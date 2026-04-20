using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStatTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private RectTransform _tooltipRect;
    [SerializeField] private Canvas _parentCanvas;

    [SerializeField] private Vector2 mouseOffset = new Vector2(15, -15);//鼠标偏移设置
    [SerializeField] private float screenPadding = 10f;//离屏幕最小距离
    private void Update()
    {
        if (!gameObject.activeSelf)
            return;
        FollowMousePosition();
    }
    public void ShowStatTooltip(string text)
    {
        description.text = text;

        gameObject.SetActive(true);
    }//显示函数
    public void HideStatTooltip()
    {
        description.text = "";
        gameObject.SetActive(false);
    }//隐藏函数

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
