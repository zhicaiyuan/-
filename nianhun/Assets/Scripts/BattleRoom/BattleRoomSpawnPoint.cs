using UnityEngine;

public class BattleRoomSpawnPoint : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = new Color(1f, 0.35f, 0.1f, 0.85f);
    [SerializeField] private float gizmoRadius = 0.35f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.6f);
    }
}
