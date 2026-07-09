using UnityEngine;

[CreateAssetMenu(fileName = "BossScreenHealthBarStyle", menuName = "UI/Boss血条样式")]
public class BossScreenHealthBarStyle : ScriptableObject
{
    [Header("布局")]
    public Vector2 anchorMin = new Vector2(0.08f, 0.04f);
    public Vector2 anchorMax = new Vector2(0.92f, 0.12f);
    [Tooltip("内层血量填充区域，比例与头顶 entity-stat-UI 血条一致")]
    public Vector2 fillAreaAnchorMin = new Vector2(0.13f, 0.30f);
    public Vector2 fillAreaAnchorMax = new Vector2(0.87f, 0.70f);

    [Header("外框（同头顶血条 bar.png）")]
    public Sprite frameSprite;
    public Color frameColor = Color.white;

    [Header("填充（同头顶血条：白缓冲 + 红血量）")]
    public Color delayFillColor = Color.white;
    public Color healthFillColor = Color.red;
    [Tooltip("留空则使用 Unity 默认 Sliced UI 图")]
    public Sprite delayFillSprite;
    public Sprite healthFillSprite;

    [Header("动画")]
    [Min(0.01f)]
    public float delayLerpSpeed = 2f;

    public static BossScreenHealthBarStyle CreateRuntimeDefault()
    {
        BossScreenHealthBarStyle style = CreateInstance<BossScreenHealthBarStyle>();
        style.frameSprite = Resources.Load<Sprite>("UI/bar");
        return style;
    }
}
