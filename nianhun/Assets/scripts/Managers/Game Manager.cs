using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour ,ISaveManager
{
    public static GameManager instance;
    [SerializeField] private Checkpoint[] checkpoints;

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
       
    }
    public void RestartScence()
    {
        Scene scene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(scene.name);
    }//重新开始场景

    public void LoadData(GameData data)
    {
        foreach(KeyValuePair<string,bool> pair in data.checkpoints)
        {
            
            foreach (Checkpoint checkpoint in checkpoints)
            {
                Debug.Log(checkpoint.id + checkpoint.activated);
                if (checkpoint.id == pair.Key && pair.Value == true)
                    checkpoint.ActiveCheckpoint();
                
            }
        }//加载存档可以激活存档点
    }

    public void SaveData(ref GameData data)
    {
        data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            data.checkpoints.Add(checkpoint.id, checkpoint.activated);
        }
    }//保存存档点
}
