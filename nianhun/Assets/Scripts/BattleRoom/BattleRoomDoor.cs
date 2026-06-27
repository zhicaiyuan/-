using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BattleRoomDoor : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openLocalPosition = new Vector3(0f, 4f, 0f);
    [SerializeField] private Vector3 closedLocalPosition = Vector3.zero;
    [SerializeField] private float moveDuration = 0.45f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Collider2D blocker;
    [SerializeField] private SpriteRenderer sprite;

    private Coroutine moveRoutine;
    private bool isClosed;

    private void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        if (blocker == null)
            blocker = GetComponent<Collider2D>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (blocker != null)
            blocker.enabled = false;
    }

    private void Start()
    {
        SetOpenInstant();
    }

    public void Open()
    {
        if (!isClosed && moveRoutine == null && IsAtOpenPosition())
            return;

        StartMove(openLocalPosition, enableBlocker: false, markClosed: false);
    }

    public void Close()
    {
        if (isClosed && moveRoutine == null && IsAtClosedPosition())
            return;

        StartMove(closedLocalPosition, enableBlocker: true, markClosed: true);
    }

    public void SetOpenInstant()
    {
        StopMoveRoutine();
        ApplyPosition(openLocalPosition);
        SetBlockerEnabled(false);
        isClosed = false;
    }

    public void SetCloseInstant()
    {
        StopMoveRoutine();
        ApplyPosition(closedLocalPosition);
        SetBlockerEnabled(true);
        isClosed = true;
    }

    private void StartMove(Vector3 targetLocalPosition, bool enableBlocker, bool markClosed)
    {
        StopMoveRoutine();

        if (!enableBlocker)
            SetBlockerEnabled(false);

        moveRoutine = StartCoroutine(MoveRoutine(targetLocalPosition, enableBlocker, markClosed));
    }

    private IEnumerator MoveRoutine(Vector3 targetLocalPosition, bool enableBlockerOnComplete, bool markClosed)
    {
        Vector3 start = doorTransform.localPosition;
        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = moveCurve != null ? moveCurve.Evaluate(t) : t;
            doorTransform.localPosition = Vector3.LerpUnclamped(start, targetLocalPosition, eased);
            yield return null;
        }

        ApplyPosition(targetLocalPosition);

        if (enableBlockerOnComplete)
            SetBlockerEnabled(true);

        isClosed = markClosed;
        moveRoutine = null;
    }

    private void ApplyPosition(Vector3 localPosition)
    {
        doorTransform.localPosition = localPosition;
    }

    private void SetBlockerEnabled(bool enabled)
    {
        if (blocker != null)
            blocker.enabled = enabled;
    }

    private void StopMoveRoutine()
    {
        if (moveRoutine == null)
            return;

        StopCoroutine(moveRoutine);
        moveRoutine = null;
    }

    private bool IsAtOpenPosition()
    {
        return Vector3.Distance(doorTransform.localPosition, openLocalPosition) < 0.01f;
    }

    private bool IsAtClosedPosition()
    {
        return Vector3.Distance(doorTransform.localPosition, closedLocalPosition) < 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        Transform target = doorTransform != null ? doorTransform : transform;
        Transform root = target.parent != null ? target.parent : target;

        Vector3 openWorld = root.TransformPoint(openLocalPosition);
        Vector3 closedWorld = root.TransformPoint(closedLocalPosition);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(openWorld, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(closedWorld, 0.2f);
        Gizmos.DrawLine(openWorld, closedWorld);
    }
}
