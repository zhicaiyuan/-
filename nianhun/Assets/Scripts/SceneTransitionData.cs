using UnityEngine;

public static class SceneTransitionData
{
    public static string PendingSpawnId { get; private set; }
    public static bool SkipCheckpointOnNextSceneLoad { get; private set; }
    public static bool ShouldFadeInAfterLoad { get; private set; }

    public static void SetTransition(string spawnId)
    {
        PendingSpawnId = spawnId;
        SkipCheckpointOnNextSceneLoad = true;
        ShouldFadeInAfterLoad = true;
    }

    public static bool ShouldHoldBlackOnLoad()
    {
        return ShouldFadeInAfterLoad;
    }

    // 返回 true = 场景切换载入（不要用存档点）；spawnApplied=false 时用场景默认入口
    public static bool TryConsumeTransition(Transform playerTransform, out bool spawnApplied)
    {
        spawnApplied = false;

        if (string.IsNullOrEmpty(PendingSpawnId) && !SkipCheckpointOnNextSceneLoad)
            return false;

        if (!string.IsNullOrEmpty(PendingSpawnId))
        {
            SceneSpawnPoint spawn = SceneSpawnPoint.FindById(PendingSpawnId);
            if (spawn != null)
            {
                playerTransform.position = spawn.transform.position;
                spawn.ApplyFacing(playerTransform.GetComponent<Player>());
                spawnApplied = true;
            }
            else
            {
                Debug.LogWarning($"未找到场景出生点: {PendingSpawnId}，将使用场景默认入口");
            }
        }

        ClearTransitionFlags();
        return true;
    }

    public static void Clear()
    {
        PendingSpawnId = null;
        SkipCheckpointOnNextSceneLoad = false;
        ShouldFadeInAfterLoad = false;
    }

    private static void ClearTransitionFlags()
    {
        PendingSpawnId = null;
        SkipCheckpointOnNextSceneLoad = false;
    }

    public static bool ConsumeFadeInRequest()
    {
        if (!ShouldFadeInAfterLoad)
            return false;

        ShouldFadeInAfterLoad = false;
        return true;
    }
}
