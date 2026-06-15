#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneCreator
{
    private const string TemplateScenePath = "Assets/Scenes/森林落叶小径.unity";
    private const string DefaultFolder = "Assets/Scenes";

    [MenuItem("Nianhun/New Game Scene")]
    private static void CreateNewGameScene()
    {
        if (!File.Exists(TemplateScenePath))
        {
            EditorUtility.DisplayDialog("创建失败", $"找不到模板场景：\n{TemplateScenePath}", "确定");
            return;
        }

        string scenePath = EditorUtility.SaveFilePanelInProject(
            "新建游戏场景",
            "新场景",
            "unity",
            "选择新场景的保存位置（会基于模板复制 Player、管理器、UI 等必要组件）");

        if (string.IsNullOrEmpty(scenePath))
            return;

        if (!AssetDatabase.CopyAsset(TemplateScenePath, scenePath))
        {
            EditorUtility.DisplayDialog("创建失败", "无法复制模板场景，请检查目标路径是否已存在。", "确定");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Scene newScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        CleanupLevelContent();
        ResetPlayerPosition();
        AddToBuildSettings(scenePath);

        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene);

        Debug.Log($"已创建游戏场景：{scenePath}");
        EditorUtility.DisplayDialog(
            "场景已创建",
            "已基于模板生成带 Player、管理器、UI、摄像机等组件的新场景。\n\n已自动清理地图、敌人、陷阱和场景切换区域，你可以直接开始搭建关卡。",
            "确定");
    }

    private static void CleanupLevelContent()
    {
        RemoveObjectsByName("Grid", "background", "陷阱区域", "切换场景区域", "SceneSpawn_森林落叶小径_入口");
        RemoveObjectsWithComponent<Enemy>();
        RemoveObjectsWithComponent<TrapZone>();
        RemoveObjectsWithComponent<ChangeSenceZone>();
        RemoveObjectsWithComponent<SceneSpawnPoint>();
        RemoveObjectsWithComponent<Checkpoint>();
    }

    private static void RemoveObjectsByName(params string[] names)
    {
        HashSet<string> targetNames = new HashSet<string>(names);

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            RemoveByNameRecursive(root.transform, targetNames);
    }

    private static void RemoveByNameRecursive(Transform current, HashSet<string> targetNames)
    {
        for (int i = current.childCount - 1; i >= 0; i--)
            RemoveByNameRecursive(current.GetChild(i), targetNames);

        if (targetNames.Contains(current.name))
            Undo.DestroyObjectImmediate(current.gameObject);
    }

    private static void RemoveObjectsWithComponent<T>() where T : Component
    {
        foreach (T component in Object.FindObjectsOfType<T>(true))
        {
            if (component == null)
                continue;

            Undo.DestroyObjectImmediate(component.gameObject);
        }
    }

    private static void ResetPlayerPosition()
    {
        Player player = Object.FindObjectOfType<Player>();
        if (player == null)
            return;

        Undo.RecordObject(player.transform, "Reset Player Position");
        player.transform.position = new Vector3(0f, 5f, 0f);
    }

    private static void AddToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(scene => scene.path == scenePath))
            return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
