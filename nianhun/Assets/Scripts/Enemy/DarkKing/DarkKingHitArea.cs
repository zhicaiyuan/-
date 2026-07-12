using System;
using UnityEngine;

public enum DarkKingHitShape
{
    Circle,
    Box
}

[Serializable]
public class DarkKingHitArea
{
    public Transform hitCheck;
    public DarkKingHitShape shape = DarkKingHitShape.Circle;
    public float radius = 1.2f;
    public Vector2 boxSize = new Vector2(2.4f, 2.4f);

    public Collider2D[] GetOverlappingColliders()
    {
        if (hitCheck == null)
            return Array.Empty<Collider2D>();

        if (shape == DarkKingHitShape.Box)
            return Physics2D.OverlapBoxAll(hitCheck.position, boxSize, hitCheck.eulerAngles.z);

        return Physics2D.OverlapCircleAll(hitCheck.position, radius);
    }

    public void DrawGizmo(Color color)
    {
        if (hitCheck == null)
            return;

        Gizmos.color = color;
        if (shape == DarkKingHitShape.Box)
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
