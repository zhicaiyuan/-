using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour ,ISaveManager
{
    public static GameManager instance;
    private Transform player;
    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private string closestCheckpointLoaded;

    [Header("失去货币")]
    [SerializeField] private GameObject lostCurrencyPerfab;
    public int lostCurrencyAmount;
    public float lostCurrencyX;
    public float lostCurrencyY;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        checkpoints = FindObjectsOfType<Checkpoint>();

        player = playermanger.instance.player.transform;
        
    }
    public void RestartScence()
    {
        SaveManager.instance.SaveGame();

        Scene scene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(scene.name);
    }//重新开始场景

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadWithDelath(data));
    }

    private void LoadCheckpoints(GameData data)
    {
        foreach (KeyValuePair<string, bool> pair in data.checkpoints)
        {

            foreach (Checkpoint checkpoint in checkpoints)
            {
                Debug.Log(checkpoint.id + checkpoint.activated);
                if (checkpoint.id == pair.Key && pair.Value == true)
                    checkpoint.ActiveCheckpoint();

            }
        }//加载存档可以激活存档点
    }
    private void Update()
    {
        Debug.Log(lostCurrencyAmount);
    }
    private void LoadLostCurrency(GameData data)
    {
        lostCurrencyAmount = data.lostCurrencyAmount;
        lostCurrencyX = data.lostCurrencyX;
        lostCurrencyY = data.lostCurrencyY;

        if (lostCurrencyAmount > 0)
        { 
            GameObject newLostCurrency = Instantiate(lostCurrencyPerfab, new Vector3(lostCurrencyX, lostCurrencyY), Quaternion.identity); 
            
            newLostCurrency.GetComponent<LostCurrency>().currency = lostCurrencyAmount;
        }

        lostCurrencyAmount = 0;
    }

    private IEnumerator LoadWithDelath(GameData data)
    {
        yield return new WaitForSeconds(.1f);

        LoadLostCurrency(data);
        PlacePlayerAtClosestCheckpoint(data);
        LoadCheckpoints(data);
    }

    private void PlacePlayerAtClosestCheckpoint(GameData data)
    {
        if(data.closestCheckpointId == null)
            return;
        closestCheckpointLoaded = data.closestCheckpointId;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (closestCheckpointLoaded == checkpoint.id)
                player.position = checkpoint.transform.position;
        }//改变角色位置
    }

    public void SaveData(ref GameData data)
    {
        data.lostCurrencyAmount = lostCurrencyAmount;
        data.lostCurrencyX = player.position.x;
        data.lostCurrencyY = player.position.y;

        if(FindClosestCheckpoint() != null)
            data.closestCheckpointId = FindClosestCheckpoint().id;
        data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            data.checkpoints.Add(checkpoint.id, checkpoint.activated);
        }

        
    }//保存存档点

    private Checkpoint FindClosestCheckpoint()
    {


        float closestDistance = Mathf.Infinity;
        Checkpoint closestcheckpoint = null;
        foreach(var checkpoint in checkpoints)
        {
            float distanceToCheckpoint = Vector2.Distance(player.position,checkpoint.transform.position);

            if(distanceToCheckpoint < closestDistance && checkpoint.activated == true)
            {
                closestDistance = distanceToCheckpoint;
                closestcheckpoint = checkpoint;
            }
        }

        return closestcheckpoint;
    }//找到最近的检查点

    public void PauseGame(bool pause)
    {
        if (pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }//暂停游戏
}
