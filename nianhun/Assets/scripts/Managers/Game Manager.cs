using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ISaveManager
{
    public static GameManager instance;
    private Transform player;
    private Checkpoint[] checkpoints;
    [SerializeField] private string closestCheckpointLoaded;

    // 如果为 true，则在下次场景加载时跳过把玩家放到存档点（用于场景切换时保持初始位置）
    public bool skipCheckpointOnNextSceneLoad = false;

    [Header("失去货币")]
    [SerializeField] private GameObject lostCurrencyPerfab;
    [SerializeField] private float corpseSearchRadius = 22f;
    [SerializeField] private float corpseStandGap = 0.15f;
    public int lostCurrencyAmount;
    public float lostCurrencyX;
    public float lostCurrencyY;

    private GameData pendingLoadData;
    private bool hasAppliedCheckpointData;
    private bool hasLoadedLostCurrency;
    private string registeredRespawnCheckpointId;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        RefreshCheckpointList();
    }

    private void Start()
    {
        player = playermanger.instance.player.transform;
        StartCoroutine(ApplyCheckpointWhenSceneReady());

        if (SceneTransitionData.ConsumeFadeInRequest())
            StartCoroutine(FadeInAfterSceneTransition());
    }

    private IEnumerator ApplyCheckpointWhenSceneReady()
    {
        yield return null;

        if (player == null && playermanger.instance != null && playermanger.instance.player != null)
            player = playermanger.instance.player.transform;

        hasAppliedCheckpointData = false;
        TryApplyCheckpointData();
    }

    public void RegisterRespawnCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return;

        registeredRespawnCheckpointId = checkpointId;

        if (SaveManager.instance != null)
            SaveManager.instance.SetClosestCheckpointId(checkpointId);
    }

    private IEnumerator FadeInAfterSceneTransition()
    {
        UIFadeScreen fadeScreen = FindObjectOfType<UIFadeScreen>();
        if (fadeScreen == null)
            yield break;

        fadeScreen.SetBlackInstant();
        yield return null;
        yield return new WaitForEndOfFrame();

        TryApplyCheckpointData();

        Player transitionPlayer = playermanger.instance != null ? playermanger.instance.player : null;
        if (transitionPlayer != null)
        {
            transitionPlayer.zerovelocity();
            transitionPlayer.statemachine.changestate(transitionPlayer.idlestate);
            transitionPlayer.isbusy = false;
        }

        yield return fadeScreen.FadeInRoutine(1.2f, 0.1f);
    }

    public void RestartScence()
    {
        PauseGame(false);
        SceneTransitionData.Clear();
        SaveManager.instance.SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadData(GameData data)
    {
        pendingLoadData = data;
        hasAppliedCheckpointData = false;
        hasLoadedLostCurrency = false;

        if (!string.IsNullOrEmpty(data.closestCheckpointId))
            registeredRespawnCheckpointId = data.closestCheckpointId;
    }

    private void TryApplyCheckpointData()
    {
        if (pendingLoadData == null || hasAppliedCheckpointData)
            return;

        if (player == null && playermanger.instance != null && playermanger.instance.player != null)
            player = playermanger.instance.player.transform;

        if (checkpoints == null || checkpoints.Length == 0)
            RefreshCheckpointList();

        LoadCheckpoints(pendingLoadData);
        LoadLostCurrency(pendingLoadData);

        if (player != null)
        {
            if (SceneTransitionData.TryConsumeTransition(player, out bool spawnApplied))
            {
                if (!spawnApplied && !skipCheckpointOnNextSceneLoad)
                    PlacePlayerAtCheckpointOrInitialSpawn(pendingLoadData);
            }
            else if (!skipCheckpointOnNextSceneLoad)
            {
                PlacePlayerAtCheckpointOrInitialSpawn(pendingLoadData);
            }
            else
            {
                skipCheckpointOnNextSceneLoad = false;
            }

            hasAppliedCheckpointData = true;
        }
    }

    private void PlacePlayerAtCheckpointOrInitialSpawn(GameData data)
    {
        if (TryPlacePlayerAtCheckpoint(data.closestCheckpointId))
            return;

        string fallbackCheckpointId = GetSingleActivatedCheckpointId(data);
        if (TryPlacePlayerAtCheckpoint(fallbackCheckpointId))
            return;

        PlacePlayerAtInitialSpawn();
    }

    private static string GetSingleActivatedCheckpointId(GameData data)
    {
        if (data.checkpoints == null)
            return null;

        string foundId = null;

        foreach (KeyValuePair<string, bool> pair in data.checkpoints)
        {
            if (!pair.Value || string.IsNullOrEmpty(pair.Key))
                continue;

            if (foundId != null)
                return null;

            foundId = pair.Key;
        }

        return foundId;
    }

    private bool TryPlacePlayerAtCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return false;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.id != checkpointId || !checkpoint.activated)
                continue;

            closestCheckpointLoaded = checkpointId;
            player.position = checkpoint.transform.position;
            return true;
        }

        return false;
    }

    private void PlacePlayerAtInitialSpawn()
    {
        SceneSpawnPoint spawnPoint = FindDefaultSpawnPoint();
        if (spawnPoint == null)
            return;

        player.position = spawnPoint.transform.position;

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
            spawnPoint.ApplyFacing(playerComponent);
    }

    private static SceneSpawnPoint FindDefaultSpawnPoint()
    {
        SceneSpawnPoint[] spawnPoints = Object.FindObjectsOfType<SceneSpawnPoint>(includeInactive: true);
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        string sceneName = SceneManager.GetActiveScene().name;

        foreach (SceneSpawnPoint spawnPoint in spawnPoints)
        {
            string spawnId = spawnPoint.SpawnId;
            if (spawnId.StartsWith(sceneName) && spawnId.Contains("入口"))
                return spawnPoint;
        }

        return spawnPoints[0];
    }

    private void RefreshCheckpointList()
    {
        checkpoints = Object.FindObjectsOfType<Checkpoint>(includeInactive: true);
    }

    private void LoadCheckpoints(GameData data)
    {
        if (data.checkpoints == null)
            return;

        foreach (KeyValuePair<string, bool> pair in data.checkpoints)
        {
            if (!pair.Value)
                continue;

            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint.id == pair.Key)
                    checkpoint.ActiveCheckpoint(playSound: false);
            }
        }
    }

    private void LoadLostCurrency(GameData data)
    {
        if (hasLoadedLostCurrency)
            return;

        hasLoadedLostCurrency = true;
        lostCurrencyAmount = data.lostCurrencyAmount;
        lostCurrencyX = data.lostCurrencyX;
        lostCurrencyY = data.lostCurrencyY;

        if (lostCurrencyAmount > 0)
        {
            Vector3 corpsePosition = ResolveCorpsePosition(new Vector3(lostCurrencyX, lostCurrencyY, 0f));
            lostCurrencyX = corpsePosition.x;
            lostCurrencyY = corpsePosition.y;

            GameObject newLostCurrency = Instantiate(lostCurrencyPerfab, corpsePosition, Quaternion.identity);
            newLostCurrency.GetComponent<LostCurrency>().currency = lostCurrencyAmount;
        }

        lostCurrencyAmount = 0;
    }

    public void SaveData(ref GameData data)
    {
        if (checkpoints == null || checkpoints.Length == 0)
            RefreshCheckpointList();

        data.lostCurrencyAmount = lostCurrencyAmount;

        if (lostCurrencyAmount > 0)
        {
            data.lostCurrencyX = lostCurrencyX;
            data.lostCurrencyY = lostCurrencyY;
        }
        else if (player != null)
        {
            data.lostCurrencyX = player.position.x;
            data.lostCurrencyY = player.position.y;
        }

        if (!string.IsNullOrEmpty(registeredRespawnCheckpointId))
            data.closestCheckpointId = registeredRespawnCheckpointId;
        else
        {
            PlayerStat playerStat = player != null ? player.GetComponent<PlayerStat>() : null;
            if (playerStat == null || !playerStat.isdead)
            {
                Checkpoint closestCheckpoint = FindClosestCheckpoint();
                if (closestCheckpoint != null && !string.IsNullOrEmpty(closestCheckpoint.id))
                    data.closestCheckpointId = closestCheckpoint.id;
            }
        }

        data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.EnsureStableId();

            if (string.IsNullOrEmpty(checkpoint.id))
                continue;

            data.checkpoints.Add(checkpoint.id, checkpoint.activated);
        }
    }

    private Checkpoint FindClosestCheckpoint()
    {
        if (player == null)
            return null;

        float closestDistance = Mathf.Infinity;
        Checkpoint closestcheckpoint = null;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            float distanceToCheckpoint = Vector2.Distance(player.position, checkpoint.transform.position);

            if (distanceToCheckpoint < closestDistance && checkpoint.activated)
            {
                closestDistance = distanceToCheckpoint;
                closestcheckpoint = checkpoint;
            }
        }

        return closestcheckpoint;
    }

    public void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
    }

    public void DropLostCurrencyCorpse(Vector3 deathPosition, Player player)
    {
        if (lostCurrencyAmount <= 0 || lostCurrencyPerfab == null)
            return;

        foreach (LostCurrency existingCorpse in Object.FindObjectsOfType<LostCurrency>())
            Destroy(existingCorpse.gameObject);

        Vector3 corpsePosition = ResolveCorpsePosition(deathPosition, player);
        lostCurrencyX = corpsePosition.x;
        lostCurrencyY = corpsePosition.y;

        GameObject newLostCurrency = Instantiate(lostCurrencyPerfab, corpsePosition, Quaternion.identity);
        newLostCurrency.GetComponent<LostCurrency>().currency = lostCurrencyAmount;
    }

    private Vector3 ResolveCorpsePosition(Vector3 rawPosition, Player player = null)
    {
        if (player == null && playermanger.instance != null)
            player = playermanger.instance.player;

        if (player == null)
            return rawPosition;

        NearestPlatformFinder.Settings settings = new NearestPlatformFinder.Settings
        {
            groundLayer = player.GroundLayer,
            bodyCollider = player.cd,
            excludeColliders = TrapZone.GetAllTrapColliders(),
            standGap = corpseStandGap,
            searchRadius = corpseSearchRadius,
            horizontalStep = 0.35f,
            probeHeight = 8f,
            maxRayDistance = 30f,
            verticalSearchBoost = 6f,
            upwardPenaltyWeight = 0.35f,
            maxUpwardFromOrigin = -1f
        };

        if (NearestPlatformFinder.TryFind(rawPosition, in settings, out Vector3 platformPosition))
            return platformPosition;

        return rawPosition;
    }
}
