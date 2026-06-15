#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class OneWayPlatformEditor
{
    [MenuItem("Nianhun/Setup One-Way Platform")]
    private static void SetupSelectedPlatforms()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("请先选中要设置成单向平台的物体。");
            return;
        }

        foreach (GameObject selected in Selection.gameObjects)
        {
            Collider2D collider = selected.GetComponent<Collider2D>();
            if (collider == null)
                collider = Undo.AddComponent<BoxCollider2D>(selected);

            DropThroughPlatform platform = selected.GetComponent<DropThroughPlatform>();
            if (platform == null)
                platform = Undo.AddComponent<DropThroughPlatform>(selected);

            EditorUtility.SetDirty(selected);
        }

        Debug.Log($"已为 {Selection.gameObjects.Length} 个物体配置单向平台。");
    }
}
#endif
