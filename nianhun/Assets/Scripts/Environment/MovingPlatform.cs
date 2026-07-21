using UnityEngine;

/// <summary>
/// 左右或上下往返移动的平台。站在上面的角色通过叠加速度跟随（见 Entity.setvelocity）。
/// 远离玩家时自动休眠，减轻大量平台同时运算的开销。
/// </summary>
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis
    {
        Horizontal,
        Vertical
    }

    [Header("路径")]
    [SerializeField] private MoveAxis axis = MoveAxis.Horizontal;
    [Tooltip("相对起点的单程距离（世界单位）")]
    [SerializeField] private float travelDistance = 4f;
    [Tooltip("勾选后沿负方向移动（左/下）")]
    [SerializeField] private bool invertDirection;

    [Header("运动")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitAtEnds = 0.3f;
    [SerializeField] private bool startFromEnd;

    [Header("性能（远离玩家时休眠）")]
    [SerializeField] private bool sleepWhenFar = true;
    [Tooltip("进入此距离内开始运动")]
    [SerializeField] private float activeDistance = 28f;
    [Tooltip("超出此距离后休眠（应略大于 Active Distance，避免边界抖动）")]
    [SerializeField] private float sleepDistance = 36f;
    [Tooltip("距离检测间隔（秒），错开检测减轻开销")]
    [SerializeField] private float distanceCheckInterval = 0.2f;

    private Rigidbody2D rb;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 target;
    private float waitTimer;
    private Vector2 lastPosition;
    private bool isSleeping;
    private float distanceCheckTimer;

    /// <summary>本帧物理步长内的位移，供站在上面的角色同步。</summary>
    public Vector2 MovementDelta { get; private set; }

    /// <summary>当前瞬时速度（世界单位/秒）。</summary>
    public Vector2 Velocity { get; private set; }

    public bool IsSleeping => isSleeping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            if (rb.interpolation == RigidbodyInterpolation2D.None)
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        CachePathPoints();
        Vector3 start = startFromEnd ? pointB : pointA;
        if (rb != null)
            rb.position = start;
        else
            transform.position = start;

        target = startFromEnd ? pointA : pointB;
        lastPosition = rb != null ? rb.position : (Vector2)transform.position;
        MovementDelta = Vector2.zero;
        Velocity = Vector2.zero;

        // 错开各平台的首次检测，避免同一帧全部算距离
        distanceCheckTimer = Random.Range(0f, Mathf.Max(0.01f, distanceCheckInterval));
        isSleeping = sleepWhenFar;
    }

    private void CachePathPoints()
    {
        pointA = transform.position;
        float sign = invertDirection ? -1f : 1f;
        Vector3 offset = axis == MoveAxis.Horizontal
            ? new Vector3(travelDistance * sign, 0f, 0f)
            : new Vector3(0f, travelDistance * sign, 0f);
        pointB = pointA + offset;
    }

    private void FixedUpdate()
    {
        if (sleepWhenFar)
            UpdateSleepState();

        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;

        if (isSleeping)
        {
            MovementDelta = Vector2.zero;
            Velocity = Vector2.zero;
            lastPosition = current;
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            MovementDelta = Vector2.zero;
            Velocity = Vector2.zero;
            lastPosition = current;
            return;
        }

        float step = Mathf.Max(0f, speed) * Time.fixedDeltaTime;
        Vector2 next = Vector2.MoveTowards(current, target, step);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, transform.position.z);

        MovementDelta = next - lastPosition;
        Velocity = Time.fixedDeltaTime > 0f ? MovementDelta / Time.fixedDeltaTime : Vector2.zero;
        lastPosition = next;

        if (Vector2.Distance(next, (Vector2)target) <= 0.01f)
        {
            target = Vector3.Distance(target, pointA) <= Vector3.Distance(target, pointB) ? pointB : pointA;
            waitTimer = Mathf.Max(0f, waitAtEnds);
        }
    }

    private void UpdateSleepState()
    {
        distanceCheckTimer -= Time.fixedDeltaTime;
        if (distanceCheckTimer > 0f)
            return;

        distanceCheckTimer = Mathf.Max(0.05f, distanceCheckInterval);

        if (!TryGetPlayerPosition(out Vector2 playerPos))
        {
            SetSleeping(true);
            return;
        }

        Vector2 platformPos = rb != null ? rb.position : (Vector2)transform.position;
        float sqrDist = (platformPos - playerPos).sqrMagnitude;

        if (isSleeping)
        {
            float wake = Mathf.Max(0.1f, activeDistance);
            if (sqrDist <= wake * wake)
                SetSleeping(false);
        }
        else
        {
            float sleep = Mathf.Max(activeDistance, sleepDistance);
            if (sqrDist >= sleep * sleep)
                SetSleeping(true);
        }
    }

    private void SetSleeping(bool sleep)
    {
        if (isSleeping == sleep)
            return;

        isSleeping = sleep;
        MovementDelta = Vector2.zero;
        Velocity = Vector2.zero;

        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;
        lastPosition = current;
    }

    private static bool TryGetPlayerPosition(out Vector2 position)
    {
        position = default;
        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        position = playermanger.instance.player.transform.position;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 a;
        Vector3 b;

        if (Application.isPlaying)
        {
            a = pointA;
            b = pointB;
        }
        else
        {
            a = transform.position;
            float sign = invertDirection ? -1f : 1f;
            Vector3 offset = axis == MoveAxis.Horizontal
                ? new Vector3(travelDistance * sign, 0f, 0f)
                : new Vector3(0f, travelDistance * sign, 0f);
            b = a + offset;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.15f);
        Gizmos.DrawWireSphere(b, 0.15f);

        if (sleepWhenFar)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, activeDistance);
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, sleepDistance);
        }
    }
}
