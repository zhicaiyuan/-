using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleEnemyType
{
    SlimeSmall,
    SlimeMedium,
    SlimeLarge,
    Skeleton,
    Mushroom,
    WalkingStick,
    RootBoss
}

[Serializable]
public class BattleEnemyPrefabEntry
{
    public BattleEnemyType enemyType;
    public GameObject prefab;
}

[Serializable]
public class BattleRoomSpawnEntry
{
    [Tooltip("怪物类型")]
    public BattleEnemyType enemyType;

    [Tooltip("该点位生成数量")]
    public int count = 1;

    [Tooltip("对应 BattleRoomController 中 spawnPoints 的下标")]
    public int spawnPointIndex;

    [Tooltip("生成位置随机偏移半径")]
    public float spawnSpread = 0.3f;
}

[Serializable]
public class BattleRoomWave
{
    [Tooltip("本波开始前等待秒数")]
    public float delayBeforeWave = 1f;

    [Tooltip("本波所有出怪配置")]
    public List<BattleRoomSpawnEntry> spawns = new List<BattleRoomSpawnEntry>();
}

[Serializable]
public class BattleRoomConfig
{
    public string roomId;
    public List<BattleRoomWave> waves = new List<BattleRoomWave>();
}
