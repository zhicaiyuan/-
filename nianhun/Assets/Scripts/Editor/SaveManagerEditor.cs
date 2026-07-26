#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (target == null)
            return;

        DrawDefaultInspector();

        EditorGUILayout.Space();

        SaveManager saveManager = target as SaveManager;
        if (saveManager == null)
            return;

        if (GUILayout.Button("删除战斗房间存档"))
        {
            if (EditorUtility.DisplayDialog(
                "删除战斗房间存档",
                "将删除 battle_rooms.json，所有战斗房间恢复为未通关状态。\n\n是否继续？",
                "删除",
                "取消"))
            {
                saveManager.DeleteBattleRoomSave();
            }
        }
    }
}
#endif
