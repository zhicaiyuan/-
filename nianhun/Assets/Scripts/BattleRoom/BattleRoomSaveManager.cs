using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BattleRoomSaveManager
{
    private const string FileName = "battle_rooms.json";

    private static BattleRoomSaveData cachedData;
    private static bool isLoaded;

    public static bool IsRoomCleared(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return false;

        EnsureLoaded();
        return cachedData.clearedRoomIds.Contains(roomId);
    }

    public static void MarkRoomCleared(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return;

        EnsureLoaded();

        if (cachedData.clearedRoomIds.Contains(roomId))
            return;

        cachedData.clearedRoomIds.Add(roomId);
        Save();
    }

    public static IReadOnlyList<string> GetClearedRoomIds()
    {
        EnsureLoaded();
        return cachedData.clearedRoomIds;
    }

    public static void DeleteSave()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        cachedData = new BattleRoomSaveData();
        isLoaded = true;
    }

    private static void EnsureLoaded()
    {
        if (isLoaded)
            return;

        Load();
    }

    private static void Load()
    {
        string fullPath = GetFullPath();
        cachedData = new BattleRoomSaveData();
        isLoaded = true;

        if (!File.Exists(fullPath))
            return;

        try
        {
            string json = File.ReadAllText(fullPath);
            BattleRoomSaveData loaded = JsonUtility.FromJson<BattleRoomSaveData>(json);

            if (loaded != null && loaded.clearedRoomIds != null)
                cachedData = loaded;
        }
        catch (Exception e)
        {
            Debug.LogError("加载战斗房间存档失败: " + fullPath + "\n" + e);
        }
    }

    private static void Save()
    {
        string fullPath = GetFullPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string json = JsonUtility.ToJson(cachedData, true);
            File.WriteAllText(fullPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("保存战斗房间存档失败: " + fullPath + "\n" + e);
        }
    }

    private static string GetFullPath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }
}
