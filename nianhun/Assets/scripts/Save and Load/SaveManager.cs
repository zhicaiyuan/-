using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [SerializeField] private string fileName;
    [SerializeField] private bool enceyptdata;

    private GameData gamedata;
    private List<ISaveManager> saveManagers;
    private FileDataHandler dataHandler;

    [ContextMenu("删除保存文件")]
    public void DeleteSaveData()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName,enceyptdata);
        dataHandler.Delete();
    }
    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath,fileName,enceyptdata);
        saveManagers = FindSaveManagers();

        LoadGame();
    }//开始时读取游戏
    public void NewGame()
    {
        gamedata = new GameData();
    }

    public void LoadGame()
    {
        gamedata = dataHandler.Load();


        if(this.gamedata == null)
        {
            Debug.Log("没有找到游戏数据");
            NewGame();
        }

        foreach(ISaveManager saveManager in saveManagers)
        {
            saveManager.LoadData(gamedata);
        }
    }//读取游戏

    public void SaveGame()
    {
        foreach(ISaveManager savemanager in saveManagers)
        {
            savemanager.SaveData(ref gamedata); 
        }

        dataHandler.Save(gamedata);
        
    }//保存游戏

    private void OnApplicationQuit()
    {
        SaveGame();
    }//游戏退出时保存

    private List<ISaveManager> FindSaveManagers()
    {
        IEnumerable<ISaveManager> saveManagers = FindObjectsOfType<MonoBehaviour>().OfType<ISaveManager>();

        return new List<ISaveManager>(saveManagers);
    }

    public bool HasSaveData()
    {
        if(dataHandler.Load() != null)
        {
            return true;
        }

        return false;
    }
}
