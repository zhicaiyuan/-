using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
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
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, enceyptdata);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        LoadGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isInitialSceneLoad)
        {
            isInitialSceneLoad = false;
            return;
        }

        cachedSaveManagers = null;
        ApplyLoadedData();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

        StartCoroutine(DeferredRefreshSkillUI());
    }

    private IEnumerator DeferredRefreshSkillUI()
    {
        yield return null;

        if (SkillManager.instance != null)
        {
            SkillManager.instance.InvalidateSlotCache();
            SkillManager.instance.RefreshAllSkillUnlocks();
        }
        else
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

        cachedSaveManagers = null;

        bool hasGameManager = false;
        bool hasInventory = false;

        foreach (ISaveManager saveManager in GetSaveManagers())
        {
            if (saveManager is GameManager)
                hasGameManager = true;
            else if (saveManager is Inventory)
                hasInventory = true;

            saveManager.SaveData(ref gamedata);
        }

        if (!hasGameManager || !hasInventory)
        {
            Debug.LogWarning(
                $"存档不完整：GameManager={hasGameManager}, Inventory={hasInventory}，" +
                $"场景={SceneManager.GetActiveScene().name}");
        }

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
            return;
        }

        if (!IsGameScene())
            return;

        gamedata.lastSceneName = SceneManager.GetActiveScene().name;
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

    public void SetClosestCheckpointId(string checkpointId)
    {
        if (gamedata == null)
            gamedata = new GameData();

        gamedata.closestCheckpointId = checkpointId ?? string.Empty;
    }

    public void ReloadFromDisk()
    {
        cachedSaveManagers = null;
        gamedata = dataHandler.Load();

        if (gamedata == null)
            NewGame();

        hasLoadedGame = true;
        ApplyLoadedData();
    }

    public string SaveFileName => fileName;

    public bool EncryptData => enceyptdata;

    public bool IsSkillUnlocked(string skillName)
    {
        if (string.IsNullOrEmpty(skillName) || gamedata?.skillTree == null)
            return false;

        return gamedata.skillTree.TryGetValue(skillName, out bool unlocked) && unlocked;
    }
}
