using UnityEngine;
using UnityEngine.UI;

public class BossScreenHealthBar : MonoBehaviour
{
    private static BossScreenHealthBar instance;
    private static BossScreenHealthBarStyle configuredStyle;
    private static Sprite builtinSlicedSprite;

    private Slider healthSlider;
    private Slider delaySlider;
    private CharaterStat boundStat;
    private RectTransform panel;
    private BossScreenHealthBarStyle style;

    public static void Configure(BossScreenHealthBarStyle styleAsset)
    {
        configuredStyle = styleAsset;

        if (instance != null)
            instance.Rebuild();
    }

    public static void Show(CharaterStat stat)
    {
        if (stat == null)
            return;

        EnsureInstance();
        instance.gameObject.SetActive(true);
        instance.Bind(stat);
    }

    public static void Hide()
    {
        if (instance == null)
            return;

        instance.Unbind();
        instance.gameObject.SetActive(false);
    }

    private void Bind(CharaterStat stat)
    {
        Unbind();
        boundStat = stat;
        boundStat.onhealthchanged += Refresh;
        Refresh();
    }

    private void Unbind()
    {
        if (boundStat != null)
            boundStat.onhealthchanged -= Refresh;

        boundStat = null;
    }

    private void Update()
    {
        if (boundStat == null || delaySlider == null || healthSlider == null || style == null)
            return;

        delaySlider.value = Mathf.Lerp(
            delaySlider.value,
            healthSlider.value,
            Time.deltaTime * style.delayLerpSpeed);
    }

    private void OnDestroy()
    {
        Unbind();
        if (instance == this)
            instance = null;
    }

    private void Refresh()
    {
        if (boundStat == null || healthSlider == null || delaySlider == null)
            return;

        healthSlider.maxValue = boundStat.Getmaxhealthvalue();
        healthSlider.value = boundStat.currenthealth;
        delaySlider.maxValue = healthSlider.maxValue;
    }

    private static BossScreenHealthBarStyle ResolveStyle()
    {
        if (configuredStyle != null)
            return configuredStyle;

        configuredStyle = Resources.Load<BossScreenHealthBarStyle>("UI/BossScreenHealthBarStyle");
        if (configuredStyle != null)
            return configuredStyle;

        configuredStyle = BossScreenHealthBarStyle.CreateRuntimeDefault();
        return configuredStyle;
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        Canvas targetCanvas = FindScreenCanvas();
        if (targetCanvas == null)
            return;

        GameObject root = new GameObject("BossScreenHealthBar");
        root.transform.SetParent(targetCanvas.transform, false);
        instance = root.AddComponent<BossScreenHealthBar>();
        instance.Rebuild();
        root.SetActive(false);
    }

    private static Canvas FindScreenCanvas()
    {
        Canvas fallback = null;
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            if (canvas.gameObject.name.Contains("UI") || canvas.GetComponent<UIIngame>() != null)
                return canvas;

            if (fallback == null)
                fallback = canvas;
        }

        return fallback;
    }

    private void Rebuild()
    {
        style = ResolveStyle();
        ClearChildren();

        panel = GetComponent<RectTransform>();
        if (panel == null)
            panel = gameObject.AddComponent<RectTransform>();

        panel.anchorMin = style.anchorMin;
        panel.anchorMax = style.anchorMax;
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = Vector2.zero;

        CreateFrameBackground();
        RectTransform fillArea = CreateFillArea();

        delaySlider = CreateFillSlider(fillArea, style.delayFillColor, style.delayFillSprite, "BossDelaySlider", 0);
        healthSlider = CreateFillSlider(fillArea, style.healthFillColor, style.healthFillSprite, "BossHealthSlider", 1);

        Refresh();
    }

    private void CreateFrameBackground()
    {
        GameObject frameObject = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.SetParent(panel, false);
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.color = style.frameColor;
        frameImage.raycastTarget = false;

        if (style.frameSprite != null)
        {
            frameImage.sprite = style.frameSprite;
            frameImage.type = Image.Type.Simple;
            frameImage.preserveAspect = false;
        }
        else
        {
            frameImage.sprite = GetFallbackSprite();
            frameImage.type = Image.Type.Simple;
        }
    }

    private RectTransform CreateFillArea()
    {
        GameObject fillAreaObject = new GameObject("FillArea", typeof(RectTransform));
        RectTransform fillArea = fillAreaObject.GetComponent<RectTransform>();
        fillArea.SetParent(panel, false);
        fillArea.anchorMin = style.fillAreaAnchorMin;
        fillArea.anchorMax = style.fillAreaAnchorMax;
        fillArea.offsetMin = Vector2.zero;
        fillArea.offsetMax = Vector2.zero;
        return fillArea;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        healthSlider = null;
        delaySlider = null;
    }

    private Slider CreateFillSlider(
        RectTransform parent,
        Color fillColor,
        Sprite fillSprite,
        string objectName,
        int siblingIndex)
    {
        GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.SetParent(parent, false);
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        sliderRect.SetSiblingIndex(siblingIndex);

        Image background = sliderObject.GetComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = false;

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.SetParent(sliderRect, false);
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.SetParent(fillAreaRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = fillColor;
        fillImage.sprite = fillSprite != null ? fillSprite : GetFallbackSprite();
        fillImage.type = Image.Type.Simple;
        fillImage.raycastTarget = false;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        return slider;
    }

    private static Sprite GetFallbackSprite()
    {
        if (builtinSlicedSprite != null)
            return builtinSlicedSprite;

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;
        builtinSlicedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
        builtinSlicedSprite.hideFlags = HideFlags.HideAndDontSave;
        return builtinSlicedSprite;
    }
}
