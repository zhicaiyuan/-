using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId;
    [SerializeField] private int faceDirection = 1;

    public string SpawnId => spawnId;

    public void ApplyFacing(Player player)
    {
        if (player == null)
            return;

        int targetDirection = faceDirection >= 0 ? 1 : -1;
        while (player.facedir != targetDirection)
            player.Flip();
    }

    public static SceneSpawnPoint FindById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (SceneSpawnPoint spawnPoint in Object.FindObjectsOfType<SceneSpawnPoint>(includeInactive: true))
        {
            if (spawnPoint.spawnId == id)
                return spawnPoint;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.35f);

        Vector3 facingEnd = transform.position + Vector3.right * (faceDirection >= 0 ? 0.8f : -0.8f);
        Gizmos.DrawLine(transform.position, facingEnd);
    }
}
