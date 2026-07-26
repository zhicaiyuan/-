using UnityEngine;

public static class DarkKingAttackWarning
{
    private static Sprite cachedWhiteSprite;
    private const string FxSortingLayer = "FX";

    public static GameObject Show(Vector3 position, Vector2 size, float duration, Color color)
    {
        GameObject warning = new GameObject("DarkKingAttackWarning");
        warning.transform.position = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        ApplyFxSorting(renderer);
        warning.transform.localScale = new Vector3(size.x, size.y, 1f);

        Object.Destroy(warning, Mathf.Max(0.05f, duration));
        return warning;
    }

    public static void ApplyFxSorting(SpriteRenderer renderer)
    {
        if (renderer == null)
            return;

        renderer.sortingLayerName = FxSortingLayer;
        if (renderer.sortingOrder < 50)
            renderer.sortingOrder = 50;
    }

    private static Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite != null)
            return cachedWhiteSprite;

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;

        cachedWhiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        cachedWhiteSprite.hideFlags = HideFlags.HideAndDontSave;
        return cachedWhiteSprite;
    }
}
