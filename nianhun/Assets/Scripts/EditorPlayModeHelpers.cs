using UnityEngine;

/// <summary>
/// 运行时切场景前清理编辑器选中，避免 Inspector 引用已销毁物体报错。
/// </summary>
public static class EditorPlayModeHelpers
{
    public static void ClearSelectionBeforeSceneLoad()
    {
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }
}
