using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class DropThroughPlatform : MonoBehaviour
{
    [SerializeField] private float surfaceArc = 180f;
    [SerializeField] private float groundStandTolerance = 0.2f;

    public Collider2D PlatformCollider { get; private set; }

    public float GroundStandTolerance => groundStandTolerance;

    private void Reset()
    {
        ConfigurePlatform();
    }

    private void Awake()
    {
        ConfigurePlatform();

        PlatformCollider = GetComponent<Collider2D>();
        if (PlatformCollider == null)
            PlatformCollider = GetComponentInChildren<Collider2D>();
    }

    private void ConfigurePlatform()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<BoxCollider2D>();

        collider.isTrigger = false;
        collider.usedByEffector = true;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
            effector = gameObject.AddComponent<PlatformEffector2D>();

        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.surfaceArc = surfaceArc;
    }

    public static bool IsDropThroughCollider(Collider2D collider)
    {
        if (collider == null)
            return false;

        DropThroughPlatform platform = collider.GetComponent<DropThroughPlatform>();
        if (platform == null)
            platform = collider.GetComponentInParent<DropThroughPlatform>();

        if (platform != null)
            return true;

        PlatformEffector2D effector = collider.GetComponent<PlatformEffector2D>();
        if (effector == null)
            effector = collider.GetComponentInParent<PlatformEffector2D>();

        return effector != null && effector.useOneWay;
    }

    public static bool CountsAsGround(RaycastHit2D hit, Collider2D entityCollider)
    {
        if (hit.collider == null)
            return false;

        if (entityCollider == null)
            return true;

        if (!IsDropThroughCollider(hit.collider))
            return true;

        DropThroughPlatform platform = hit.collider.GetComponent<DropThroughPlatform>()
            ?? hit.collider.GetComponentInParent<DropThroughPlatform>();

        float tolerance = platform != null ? platform.GroundStandTolerance : 0.2f;
        float playerBottom = entityCollider.bounds.min.y;

        if (hit.normal.y > 0.1f)
            return playerBottom >= hit.point.y - tolerance;

        float platformTop = hit.collider.bounds.max.y;
        return playerBottom >= platformTop - tolerance;
    }
}
