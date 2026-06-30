using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData 
{
    public int currency;

    public SerializableDictionary<string, bool> skillTree;
    public SerializableDictionary<string, int> inventory;
    public List<string> equipmentID;

    public SerializableDictionary<string, bool> checkpoints;
    public SerializableDictionary<string, bool> openedChests;
    public string closestCheckpointId;

    public float lostCurrencyX;
    public float lostCurrencyY;
    public int lostCurrencyAmount;

    public SerializableDictionary<string, float> volumeSettings;

    public string lastSceneName;
    public float playerX;
    public float playerY;
    public bool hasSavedPlayerPosition;

    public GameData()
    {
        this.lostCurrencyAmount = 0;
        this.lostCurrencyX = 0;
        this.lostCurrencyY = 0;


        this.currency = 0;  
        inventory = new SerializableDictionary<string, int>();
        equipmentID = new List<string>();

        checkpoints = new SerializableDictionary<string, bool>();
        openedChests = new SerializableDictionary<string, bool>();
        closestCheckpointId = string.Empty;

        volumeSettings = new SerializableDictionary<string, float>();

        skillTree = new SerializableDictionary<string, bool>();
    }
}
