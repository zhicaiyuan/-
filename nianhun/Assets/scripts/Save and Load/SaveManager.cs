using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [SerializeField] private string fileName;
    [SerializeField] private bool enceyptdata;

    private GameData gamedata;
    private FileDataHandler dataHandler;
    private List<ISaveManager> cachedSaveManagers;
    private bool hasLoadedGame;
    private bool isInitialSceneLoad = true;

    [ContextMenu("删除保存文件")]
    public void DeleteSaveData()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, enceyptdata);
        dataHandler.Delete();
        BattleRoomSaveManager.DeleteSave();
    }

    [ContextMenu("删除战斗房间存档")]
    public void DeleteBattleRoomSave()
    {
        BattleRoomSaveManager.DeleteSave();
        RefreshBattleRoomsAfterSaveDelete();
        Debug.Log("已删除战斗房间存档: battle_rooms.json");
    }

    private static void RefreshBattleRoomsAfterSaveDelete()
    {
        foreach (BattleRoomController room in Object.FindObjectsOfType<BattleRoomController>(true))
            room.ResetFromSave();
    }

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, enceyptdata);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        isInitialSceneLoad = false;
        LoadGame();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isInitialSceneLoad)
            return;

        cachedSaveManagers = null;
        ApplyLoadedData();
    }

    public void NewGame()
    {
        gamedata = new GameData();
    }

    public void LoadGame()
    {
        if (hasLoadedGame && gamedata != null)
        {
            ApplyLoadedData();
            return;
        }

        gamedata = dataHandler.Load();

        if (gamedata == null)
        {
            Debug.Log("没有找到游戏数据");
            NewGame();
        }

        hasLoadedGame = true;
        ApplyLoadedData();
    }

    private void ApplyLoadedData()
    {
        if (gamedata == null)
            return;

        foreach (ISaveManager saveManager in GetSaveManagers())
            saveManager.LoadData(gamedata);

        RefreshSkillUnlocks();
        RefreshSkillSlotVisuals();
    }

    public void SaveGame()
    {
        SaveGame(null);
    }

    public void SaveGame(string nextSceneName)
    {
        if (gamedata == null)
            gamedata = new GameData();

        foreach (ISaveManager saveManager in GetSaveManagers())
            saveManager.SaveData(ref gamedata);

        RecordSceneProgress(nextSceneName);
        dataHandler.Save(gamedata);
    }

    private void RecordSceneProgress(string nextSceneName)
    {
        if (gamedata == null)
            return;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            gamedata.lastSceneName = nextSceneName;
            gamedata.hasSavedPlayerPosition = false;
            return;
        }

        if (!IsGameScene())
            return;

        gamedata.lastSceneName = SceneManager.GetActiveScene().name;
        gamedata.hasSavedPlayerPosition = true;
    }

    public string GetContinueSceneName(string fallbackSceneName)
    {
        if (gamedata != null && !string.IsNullOrEmpty(gamedata.lastSceneName))
            return gamedata.lastSceneName;

        GameData loaded = dataHandler.Load();
        if (loaded != null && !string.IsNullOrEmpty(loaded.lastSceneName))
            return loaded.lastSceneName;

        return fallbackSceneName;
    }

    private bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name != "主菜单";
    }

    private void OnApplicationQuit()
    {
        if (IsGameScene())
            SaveGame();
    }

    private List<ISaveManager> GetSaveManagers()
    {
        if (cachedSaveManagers != null)
            return cachedSaveManagers;

        cachedSaveManagers = new List<ISaveManager>();

        foreach (MonoBehaviour behaviour in Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true))
        {
            if (behaviour is ISaveManager saveManager)
                cachedSaveManagers.Add(saveManager);
        }

        return cachedSaveManagers;
    }

    private static void RefreshSkillUnlocks()
    {
        foreach (Skill skill in Object.FindObjectsOfType<Skill>(includeInactive: true))
            skill.RefreshUnlock();
    }

    private static void RefreshSkillSlotVisuals()
    {
        foreach (UISkilltreeSlot slot in Object.FindObjectsOfType<UISkilltreeSlot>(includeInactive: true))
            slot.RefreshVisual();
    }

    public bool HasSaveData()
    {
        if (gamedata != null)
            return true;

        return dataHandler.Load() != null;
    }
}
