using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Boss 房间：进入播过场 → 召唤 Boss 并关门 → Boss 死亡播过场 → 开门并记入存档。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossRoomController : MonoBehaviour
{
    [Header("房间标识")]
    [SerializeField] private string roomId;

    [Header("触发")]
    [SerializeField] private Collider2D triggerZone;

    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private bool showBossHealthBar = true;

    [Header("门")]
    [SerializeField] private BattleRoomDoor[] doors;

    [Header("进入过场")]
    [Tooltip("进入后先走到该点（可空）")]
    [SerializeField] private Transform enterWalkTarget;
    [SerializeField] private float walkReachDistance = 0.3f;
    [SerializeField] private float maxWalkTime = 3f;
    [SerializeField] private float enterWalkSpeedMultiplier = 0.45f;
    [Tooltip("进入过场总时长（含走路后的停留，用于播动画/镜头）")]
    [SerializeField] private float enterCutsceneDuration = 2f;
    [Tooltip("Boss 生成并关门后，再过多少秒才把控制权还给玩家")]
    [SerializeField] private float postSpawnHold = 0.8f;

    [Header("死亡过场")]
    [Tooltip("Boss 死亡后等待销毁/消散的最长时间")]
    [SerializeField] private float maxDeathWait = 3f;
    [Tooltip("死亡过场时长（开门前）")]
    [SerializeField] private float deathCutsceneDuration = 2f;
    [SerializeField] private bool lockPlayerDuringDeathCutscene = true;

    [Header("音频")]
    [SerializeField] private int battleBgmIndex = 10;
    [SerializeField] private int battleCompleteSfxIndex = 35;

    [Header("事件（可绑 Animator / Timeline / 镜头）")]
    [SerializeField] private UnityEvent onEnterCutscene;
    [SerializeField] private UnityEvent onBossSpawned;
    [SerializeField] private UnityEvent onDeathCutscene;
    [SerializeField] private UnityEvent onBossDefeated;

    private bool isCleared;
    private bool isActive;
    private Coroutine bossRoutine;
    private Enemy spawnedBoss;
    private int previousBgmIndex;
    private bool isBattleMusicPlaying;

    public bool IsCleared => isCleared;
    public bool IsActive => isActive;
    public string RoomId => roomId;

    private void Awake()
    {
        if (triggerZone == null)
            triggerZone = GetComponent<Collider2D>();

        if (triggerZone != null)
            triggerZone.isTrigger = true;

        if (bossSpawnPoint == null)
            bossSpawnPoint = transform;
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
        TryStartIfPlayerInside();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision != null ? collision.GetComponent<Player>() : null;
        if (player == null)
            return;

        StartBossEncounter(player);
    }

    private void TryStartIfPlayerInside()
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
            Player player = hits[i] != null ? hits[i].GetComponent<Player>() : null;
            if (player == null)
                continue;

            StartBossEncounter(player);
            return;
        }
    }

    [ContextMenu("生成房间ID")]
    private void GenerateRoomId()
    {
        roomId = System.Guid.NewGuid().ToString();
    }

    public void StartBossEncounter(Player player)
    {
        if (isCleared || isActive || player == null)
            return;

        if (bossPrefab == null)
        {
            Debug.LogWarning("Boss 房间未配置 Boss 预制体: " + name, this);
            return;
        }

        isActive = true;
        bossRoutine = StartCoroutine(BossRoutine(player));
    }

    private IEnumerator BossRoutine(Player player)
    {
        // —— 进入过场 ——
        player.isbusy = true;
        player.zerovelocity();
        onEnterCutscene?.Invoke();

        yield return WalkToEnterTarget(player);

        if (enterCutsceneDuration > 0f)
            yield return new WaitForSeconds(enterCutsceneDuration);

        // —— 召唤 Boss + 关门 ——
        SpawnBoss();
        CloseDoors();
        StartBattleMusic();
        onBossSpawned?.Invoke();

        if (showBossHealthBar && spawnedBoss != null && spawnedBoss.Stat != null)
            BossScreenHealthBar.Show(spawnedBoss.Stat);

        if (postSpawnHold > 0f)
            yield return new WaitForSeconds(postSpawnHold);

        // —— 开战 ——
        ReleasePlayerControl(player);

        while (spawnedBoss != null && !spawnedBoss.isDead)
            yield return null;

        // 等死亡表现 / Destroy
        float deathElapsed = 0f;
        while (spawnedBoss != null && deathElapsed < maxDeathWait)
        {
            deathElapsed += Time.deltaTime;
            yield return null;
        }

        BossScreenHealthBar.Hide();

        // —— 死亡过场 ——
        if (lockPlayerDuringDeathCutscene && player != null)
        {
            player.isbusy = true;
            player.zerovelocity();
        }

        onDeathCutscene?.Invoke();

        if (deathCutsceneDuration > 0f)
            yield return new WaitForSeconds(deathCutsceneDuration);

        CompleteEncounter(player);
    }

    private IEnumerator WalkToEnterTarget(Player player)
    {
        if (enterWalkTarget == null || player == null)
            yield break;

        float dir = Mathf.Sign(enterWalkTarget.position.x - player.transform.position.x);
        if (dir == 0f)
            dir = player.facedir;

        player.autowalkstate.SetWalkDirection(dir);
        player.autowalkstate.SetSpeedMultiplier(enterWalkSpeedMultiplier);
        player.autowalkstate.SetLockVerticalVelocity(true);
        player.statemachine.changestate(player.autowalkstate);

        float elapsed = 0f;
        while (elapsed < maxWalkTime)
        {
            float distance = Mathf.Abs(player.transform.position.x - enterWalkTarget.position.x);
            if (distance <= walkReachDistance)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.zerovelocity();
    }

    private void SpawnBoss()
    {
        Vector3 position = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        GameObject instance = Instantiate(bossPrefab, position, bossPrefab.transform.rotation);
        spawnedBoss = instance.GetComponent<Enemy>();
        if (spawnedBoss == null)
            spawnedBoss = instance.GetComponentInChildren<Enemy>();

        if (spawnedBoss == null)
        {
            Debug.LogWarning("Boss 预制体上未找到 Enemy 组件: " + bossPrefab.name, this);
            return;
        }

        // 生成后立刻朝向玩家，避免开场背对
        if (playermanger.instance != null && playermanger.instance.player != null
            && spawnedBoss is RootBoss rootBoss)
        {
            rootBoss.FacePlayer(playermanger.instance.player.transform.position.x);
        }
    }

    private void CompleteEncounter(Player player)
    {
        isActive = false;
        isCleared = true;
        bossRoutine = null;
        spawnedBoss = null;

        BattleRoomSaveManager.MarkRoomCleared(roomId);
        OpenDoors();
        EndBattleMusic(playCompleteSfx: true);
        onBossDefeated?.Invoke();

        if (player != null)
            ReleasePlayerControl(player);

        if (triggerZone != null)
            triggerZone.enabled = false;
    }

    private static void ReleasePlayerControl(Player player)
    {
        if (player == null)
            return;

        player.isbusy = false;

        if (player.statemachine != null
            && player.statemachine.currentstate == player.autowalkstate)
        {
            player.statemachine.changestate(player.idlestate);
        }
    }

    private void HandleAlreadyCleared()
    {
        if (triggerZone != null)
            triggerZone.enabled = false;

        SetDoorsOpenInstant();
        onBossDefeated?.Invoke();
    }

    public void ResetFromSave()
    {
        if (bossRoutine != null)
        {
            StopCoroutine(bossRoutine);
            bossRoutine = null;
        }

        if (spawnedBoss != null)
        {
            Destroy(spawnedBoss.gameObject);
            spawnedBoss = null;
        }

        BossScreenHealthBar.Hide();
        EndBattleMusic(playCompleteSfx: false);

        isCleared = false;
        isActive = false;

        if (triggerZone != null)
            triggerZone.enabled = true;

        SetDoorsOpenInstant();
        TryStartIfPlayerInside();
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
        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 0.35f);
        }

        if (enterWalkTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(enterWalkTarget.position, 0.25f);
        }
    }
}
