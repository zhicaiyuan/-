using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BattleRoomController : MonoBehaviour
{
    [Header("房间标识")]
    [SerializeField] private string roomId;

    [Header("触发与范围")]
    [SerializeField] private Collider2D triggerZone;
    [Tooltip("用于检测波次是否清空的战斗区域，建议比触发区略大")]
    [SerializeField] private Collider2D battleBounds;

    [Header("出怪点位")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("怪物预制体映射")]
    [SerializeField] private BattleEnemyPrefabEntry[] enemyPrefabs;

    [Header("波次配置")]
    [SerializeField] private List<BattleRoomWave> waves = new List<BattleRoomWave>();
    [SerializeField] private float delayBetweenWaves = 2f;
    [SerializeField] private float enemyCheckInterval = 0.25f;

    [Header("门")]
    [SerializeField] private BattleRoomDoor[] doors;

    [Header("音频")]
    [SerializeField] private int battleBgmIndex = 10;
    [SerializeField] private int battleCompleteSfxIndex = 35;

    [Header("事件")]
    [SerializeField] private UnityEvent onBattleStart;
    [SerializeField] private UnityEvent<int> onWaveStart;
    [SerializeField] private UnityEvent<int> onWaveComplete;
    [SerializeField] private UnityEvent onBattleComplete;

    private readonly List<BattleRoomTrackedEnemy> trackedEnemies = new List<BattleRoomTrackedEnemy>();
    private readonly HashSet<int> preBattleEnemyIds = new HashSet<int>();
    private readonly Dictionary<BattleEnemyType, GameObject> prefabLookup = new Dictionary<BattleEnemyType, GameObject>();

    private bool isCleared;
    private bool isActive;
    private int currentWaveAliveCount;
    private Coroutine battleRoutine;
    private int previousBgmIndex;
    private bool isBattleMusicPlaying;

    public bool IsCleared => isCleared;
    public bool IsActive => isActive;
    public string RoomId => roomId;

    private void Awake()
    {
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        triggerZone.isTrigger = true;

        if (battleBounds == null)
            battleBounds = triggerZone;

        BuildPrefabLookup();
    }

    private void Start()
    {
        isCleared = BattleRoomSaveManager.IsRoomCleared(roomId);

        if (isCleared)
        {
            HandleAlreadyCleared();
            return;
        }

        SetDoorsOpenInstant();
        TryStartBattleIfPlayerInside();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsPlayerCollider(collision))
            return;

        StartBattle();
    }

    private void TryStartBattleIfPlayerInside()
    {
        if (triggerZone == null)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        Collider2D[] hits = new Collider2D[8];
        int count = Physics2D.OverlapCollider(triggerZone, filter, hits);

        for (int i = 0; i < count; i++)
        {
            if (!IsPlayerCollider(hits[i]))
                continue;

            StartBattle();
            return;
        }
    }

    private static bool IsPlayerCollider(Collider2D collision)
    {
        return collision != null && collision.GetComponent<Player>() != null;
    }

    [ContextMenu("生成房间ID")]
    private void GenerateRoomId()
    {
        roomId = System.Guid.NewGuid().ToString();
    }

    public void StartBattle()
    {
        if (isCleared || isActive)
            return;

        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning("战斗房间未配置波次: " + name, this);
            return;
        }

        isActive = true;
        CachePreBattleEnemies();
        CloseDoors();
        StartBattleMusic();
        onBattleStart?.Invoke();
        battleRoutine = StartCoroutine(BattleRoutine());
    }

    public void NotifyEnemyDefeated(BattleRoomTrackedEnemy tracker)
    {
        if (trackedEnemies.Remove(tracker) && currentWaveAliveCount > 0)
            currentWaveAliveCount--;
    }

    private IEnumerator BattleRoutine()
    {
        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            BattleRoomWave wave = waves[waveIndex];

            if (wave.delayBeforeWave > 0f)
                yield return new WaitForSeconds(wave.delayBeforeWave);

            onWaveStart?.Invoke(waveIndex);
            currentWaveAliveCount = 0;
            int spawnedCount = SpawnWave(wave);

            if (spawnedCount == 0)
            {
                Debug.LogWarning("战斗房间本波未生成任何怪物，请检查波次与预制体配置: " + name, this);
                AbortBattle();
                yield break;
            }

            yield return WaitUntilWaveCleared();

            onWaveComplete?.Invoke(waveIndex);

            if (waveIndex < waves.Count - 1 && delayBetweenWaves > 0f)
                yield return new WaitForSeconds(delayBetweenWaves);
        }

        CompleteBattle();
    }

    private IEnumerator WaitUntilWaveCleared()
    {
        WaitForSeconds wait = new WaitForSeconds(enemyCheckInterval);

        while (!IsWaveCleared())
            yield return wait;
    }

    private bool IsWaveCleared()
    {
        RegisterUntrackedWaveEnemiesInBounds();

        trackedEnemies.RemoveAll(enemy => enemy == null);

        if (currentWaveAliveCount > 0 || trackedEnemies.Count > 0)
            return false;

        return !HasLivingWaveEnemyInBounds();
    }

    private void CachePreBattleEnemies()
    {
        preBattleEnemyIds.Clear();

        foreach (Enemy enemy in FindEnemiesInBounds())
        {
            if (enemy != null)
                preBattleEnemyIds.Add(enemy.GetInstanceID());
        }
    }

    private void RegisterUntrackedWaveEnemiesInBounds()
    {
        foreach (Enemy enemy in FindEnemiesInBounds())
        {
            if (enemy == null || enemy.isDead)
                continue;

            if (preBattleEnemyIds.Contains(enemy.GetInstanceID()))
                continue;

            if (enemy.GetComponent<BattleRoomTrackedEnemy>() != null)
                continue;

            TrackEnemy(enemy.gameObject);
        }
    }

    private void TrackEnemy(GameObject enemyObject)
    {
        BattleRoomTrackedEnemy tracker = enemyObject.GetComponent<BattleRoomTrackedEnemy>();
        if (tracker == null)
            tracker = enemyObject.AddComponent<BattleRoomTrackedEnemy>();

        if (trackedEnemies.Contains(tracker))
            return;

        tracker.Init(this);
        trackedEnemies.Add(tracker);
        currentWaveAliveCount++;
    }

    private bool HasLivingWaveEnemyInBounds()
    {
        foreach (Enemy enemy in FindEnemiesInBounds())
        {
            if (enemy == null || enemy.isDead)
                continue;

            if (preBattleEnemyIds.Contains(enemy.GetInstanceID()))
                continue;

            return true;
        }

        return false;
    }

    private IEnumerable<Enemy> FindEnemiesInBounds()
    {
        if (battleBounds == null)
            yield break;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        Collider2D[] hits = new Collider2D[64];
        int count = Physics2D.OverlapCollider(battleBounds, filter, hits);

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null)
                yield return enemy;
        }
    }

    private int SpawnWave(BattleRoomWave wave)
    {
        int spawnedCount = 0;

        if (wave.spawns == null)
            return spawnedCount;

        foreach (BattleRoomSpawnEntry spawn in wave.spawns)
        {
            if (spawn.count <= 0)
                continue;

            if (!TryGetPrefab(spawn.enemyType, out GameObject prefab))
            {
                Debug.LogWarning("战斗房间缺少怪物预制体: " + spawn.enemyType, this);
                continue;
            }

            Transform spawnPoint = GetSpawnPoint(spawn.spawnPointIndex);
            if (spawnPoint == null)
            {
                Debug.LogWarning("战斗房间出怪点位无效, index = " + spawn.spawnPointIndex, this);
                continue;
            }

            for (int i = 0; i < spawn.count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * spawn.spawnSpread;
                Vector3 position = spawnPoint.position + (Vector3)offset;

                GameObject instance = Instantiate(prefab, position, Quaternion.identity);
                TrackEnemy(instance);
                spawnedCount++;
            }
        }

        return spawnedCount;
    }

    private Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        if (index < 0 || index >= spawnPoints.Length)
            return spawnPoints[0];

        return spawnPoints[index];
    }

    private bool TryGetPrefab(BattleEnemyType enemyType, out GameObject prefab)
    {
        return prefabLookup.TryGetValue(enemyType, out prefab) && prefab != null;
    }

    private void BuildPrefabLookup()
    {
        prefabLookup.Clear();

        if (enemyPrefabs == null)
            return;

        foreach (BattleEnemyPrefabEntry entry in enemyPrefabs)
        {
            if (entry == null || entry.prefab == null)
                continue;

            prefabLookup[entry.enemyType] = entry.prefab;
        }
    }

    private void CompleteBattle()
    {
        isActive = false;
        isCleared = true;
        battleRoutine = null;

        BattleRoomSaveManager.MarkRoomCleared(roomId);
        OpenDoors();
        EndBattleMusic(playCompleteSfx: true);
        onBattleComplete?.Invoke();
    }

    private void AbortBattle()
    {
        isActive = false;
        battleRoutine = null;
        trackedEnemies.Clear();
        currentWaveAliveCount = 0;
        OpenDoors();
        EndBattleMusic(playCompleteSfx: false);
    }

    private void HandleAlreadyCleared()
    {
        if (triggerZone != null)
            triggerZone.enabled = false;

        SetDoorsOpenInstant();
        onBattleComplete?.Invoke();
    }

    public void ResetFromSave()
    {
        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
        }

        isCleared = false;
        isActive = false;
        currentWaveAliveCount = 0;
        trackedEnemies.Clear();
        preBattleEnemyIds.Clear();

        if (triggerZone != null)
            triggerZone.enabled = true;

        SetDoorsOpenInstant();
        TryStartBattleIfPlayerInside();
    }

    private void CloseDoors()
    {
        if (doors == null)
            return;

        foreach (BattleRoomDoor door in doors)
        {
            if (door != null)
                door.Close();
        }
    }

    private void OpenDoors()
    {
        if (doors == null)
            return;

        foreach (BattleRoomDoor door in doors)
        {
            if (door != null)
                door.Open();
        }
    }

    private void SetDoorsOpenInstant()
    {
        if (doors == null)
            return;

        foreach (BattleRoomDoor door in doors)
        {
            if (door != null)
                door.SetOpenInstant();
        }
    }

    private void StartBattleMusic()
    {
        if (AudioManager.instance == null || isBattleMusicPlaying)
            return;

        previousBgmIndex = AudioManager.instance.bgmIndex;
        isBattleMusicPlaying = true;
        AudioManager.instance.PlayBattleBgm(battleBgmIndex);
    }

    private void EndBattleMusic(bool playCompleteSfx)
    {
        if (!isBattleMusicPlaying || AudioManager.instance == null)
            return;

        isBattleMusicPlaying = false;
        AudioManager.instance.RestoreBgm(previousBgmIndex);

        if (playCompleteSfx)
            AudioManager.instance.PlaySFX(battleCompleteSfxIndex, null);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(spawnPoints[i].position, 0.25f);
        }
    }
}
