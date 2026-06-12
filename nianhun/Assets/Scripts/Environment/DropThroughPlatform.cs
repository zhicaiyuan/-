using UnityEngine;

public class DropThroughPlatform : MonoBehaviour
{
    public Collider2D PlatformCollider { get; private set; }

    private void Awake()
    {
        PlatformCollider = GetComponent<Collider2D>();
        if (PlatformCollider == null)
            PlatformCollider = GetComponentInChildren<Collider2D>();
    }

    public static bool IsDropThroughCollider(Collider2D collider)
    {
        if (collider == null)
            return false;

        if (collider.GetComponentInParent<DropThroughPlatform>() != null)
            return true;

        PlatformEffector2D effector = collider.GetComponent<PlatformEffector2D>();
        if (effector == null)
            effector = collider.GetComponentInParent<PlatformEffector2D>();

        return effector != null && effector.useOneWay;
    }
}
