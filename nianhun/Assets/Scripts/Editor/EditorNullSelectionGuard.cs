#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 换场景 / 退出 Play 时，Inspector 仍引用已销毁物体会抛
/// GameObjectInspector.OnDisable / SerializedObjectNotCreatableException。
/// 在这些时机清掉无效选中。
/// </summary>
[InitializeOnLoad]
internal static class EditorNullSelectionGuard
{
    static EditorNullSelectionGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.sceneOpening += OnSceneOpening;
        EditorApplication.hierarchyChanged += CleanupInvalidSelection;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode
            || state == PlayModeStateChange.ExitingEditMode)
        {
            Selection.activeObject = null;
        }
    }

    private static void OnSceneOpening(string path, OpenSceneMode mode)
    {
        Selection.activeObject = null;
    }

    private static void CleanupInvalidSelection()
    {
        Object[] selected = Selection.objects;
        if (selected == null || selected.Length == 0)
            return;

        Object[] valid = selected.Where(obj => obj != null).ToArray();
        if (valid.Length != selected.Length)
            Selection.objects = valid;
    }
}
#endif
