using System.Collections;
using UnityEngine;

/// <summary>
/// 拉杆控制的门：打开时相对关闭位置偏移移动，并关闭阻挡碰撞。
/// </summary>
public class LeverDoor : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [Tooltip("相对关闭位置的打开偏移（默认向上抬起）")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 4f, 0f);
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Collider2D blocker;

    private Vector3 closedLocalPosition;
    private Coroutine moveRoutine;
    private bool isOpen;
    private bool capturedClosed;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        if (blocker == null)
            blocker = GetComponent<Collider2D>();

        CaptureClosedPosition();
    }

    private void CaptureClosedPosition()
    {
        if (capturedClosed || doorTransform == null)
            return;

        closedLocalPosition = doorTransform.localPosition;
        capturedClosed = true;
    }

    private Vector3 OpenLocalPosition => closedLocalPosition + openOffset;

    public void Open(bool instant = false)
    {
        CaptureClosedPosition();

        if (isOpen && moveRoutine == null && IsNear(OpenLocalPosition))
            return;

        if (instant)
        {
            StopMoveRoutine();
            ApplyPosition(OpenLocalPosition);
            SetBlockerEnabled(false);
            isOpen = true;
            return;
        }

        StartMove(OpenLocalPosition, enableBlockerOnComplete: false, markOpen: true);
    }

    public void Close(bool instant = false)
    {
        CaptureClosedPosition();

        if (!isOpen && moveRoutine == null && IsNear(closedLocalPosition))
            return;

        if (instant)
        {
            StopMoveRoutine();
            ApplyPosition(closedLocalPosition);
            SetBlockerEnabled(true);
            isOpen = false;
            return;
        }

        StartMove(closedLocalPosition, enableBlockerOnComplete: true, markOpen: false);
    }

    private void StartMove(Vector3 targetLocalPosition, bool enableBlockerOnComplete, bool markOpen)
    {
        StopMoveRoutine();

        if (!enableBlockerOnComplete)
            SetBlockerEnabled(false);

        moveRoutine = StartCoroutine(MoveRoutine(targetLocalPosition, enableBlockerOnComplete, markOpen));
    }

    private IEnumerator MoveRoutine(Vector3 targetLocalPosition, bool enableBlockerOnComplete, bool markOpen)
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

        isOpen = markOpen;
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

    private bool IsNear(Vector3 localPosition)
    {
        return Vector3.Distance(doorTransform.localPosition, localPosition) < 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        Transform target = doorTransform != null ? doorTransform : transform;
        Vector3 closed = Application.isPlaying && capturedClosed
            ? closedLocalPosition
            : target.localPosition;
        Vector3 open = closed + openOffset;

        Vector3 openWorld = target.parent != null ? target.parent.TransformPoint(open) : open;
        Vector3 closedWorld = target.parent != null ? target.parent.TransformPoint(closed) : closed;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(openWorld, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(closedWorld, 0.2f);
        Gizmos.DrawLine(closedWorld, openWorld);
    }
}
