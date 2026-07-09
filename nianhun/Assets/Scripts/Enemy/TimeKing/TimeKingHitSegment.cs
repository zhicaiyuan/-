using System;
using UnityEngine;

public enum TimeKingHitShape
{
    Circle,
    Box
}

[Serializable]
public class TimeKingHitArea
{
    public Transform hitCheck;
    public TimeKingHitShape shape = TimeKingHitShape.Circle;
    public float radius = 1f;
    [Tooltip("矩形判定尺寸（宽×高，随 hitCheck 旋转）")]
    public Vector2 boxSize = new Vector2(2f, 1f);

    public Collider2D[] GetOverlappingColliders()
    {
        if (hitCheck == null)
            return Array.Empty<Collider2D>();

        if (shape == TimeKingHitShape.Box)
            return Physics2D.OverlapBoxAll(hitCheck.position, boxSize, hitCheck.eulerAngles.z);

        return Physics2D.OverlapCircleAll(hitCheck.position, radius);
    }

    public void DrawGizmo(Color color)
    {
        if (hitCheck == null)
            return;

        Gizmos.color = color;

        if (shape == TimeKingHitShape.Box)
        {
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(hitCheck.position, hitCheck.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = previous;
            return;
        }

        Gizmos.DrawWireSphere(hitCheck.position, radius);
    }
}

[Serializable]
public class TimeKingHitSegment : TimeKingHitArea
{
    public float hitTime = 0.5f;
    public float damageMultiplier = 1f;
}
