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
        TryApplyCheckpointData();

        if (SceneTransitionData.ConsumeFadeInRequest())
            StartCoroutine(FadeInAfterSceneTransition());
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
        SaveManager.instance.SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadData(GameData data)
    {
        pendingLoadData = data;
        hasAppliedCheckpointData = false;
        hasLoadedLostCurrency = false;
        TryApplyCheckpointData();
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
            if (SceneTransitionData.TryConsumeTransition(player, out _))
            {
                // 场景切换出生点已处理，跳过存档点
            }
            else if (TryPlacePlayerAtSavedPosition(pendingLoadData))
            {
                // 从主菜单继续时使用保存时的位置
            }
            else if (!skipCheckpointOnNextSceneLoad)
            {
                PlacePlayerAtClosestCheckpoint(pendingLoadData);
            }
            else
            {
                skipCheckpointOnNextSceneLoad = false;
            }

            hasAppliedCheckpointData = true;
        }
    }

    private bool TryPlacePlayerAtSavedPosition(GameData data)
    {
        if (!data.hasSavedPlayerPosition)
            return false;

        if (string.IsNullOrEmpty(data.lastSceneName) || data.lastSceneName != SceneManager.GetActiveScene().name)
            return false;

        player.position = new Vector3(data.playerX, data.playerY, player.position.z);
        return true;
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

    private void PlacePlayerAtClosestCheckpoint(GameData data)
    {
        if (string.IsNullOrEmpty(data.closestCheckpointId))
            return;

        closestCheckpointLoaded = data.closestCheckpointId;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (closestCheckpointLoaded == checkpoint.id)
                player.position = checkpoint.transform.position;
        }
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

        Checkpoint closestCheckpoint = FindClosestCheckpoint();
        if (closestCheckpoint != null)
            data.closestCheckpointId = closestCheckpoint.id;

        if (player != null)
        {
            data.playerX = player.position.x;
            data.playerY = player.position.y;
        }

        data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
            data.checkpoints.Add(checkpoint.id, checkpoint.activated);
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
