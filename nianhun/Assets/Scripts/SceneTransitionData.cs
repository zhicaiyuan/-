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
                Debug.LogWarning($"未找到场景出生点: {PendingSpawnId}");
            }
        }

        ClearTransitionFlags();
        return true;
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
