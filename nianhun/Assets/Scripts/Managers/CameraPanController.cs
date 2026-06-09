using Cinemachine;
using UnityEngine;

public class CameraPanController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float panSpeed = 12f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private Vector2 maxPanOffset = new Vector2(5f, 3f);
    [SerializeField] private bool returnToCenterWhenReleased = true;

    private Vector2 panOffset;
    private Vector3 baseTrackedOffset;
    private Vector3 baseFollowOffset;
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineTransposer transposer;
    private bool usesFramingTransposer;

    private void Awake()
    {
        if (virtualCamera == null)
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogWarning("CameraPanController: 未找到 CinemachineVirtualCamera。");
            return;
        }

        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            usesFramingTransposer = true;
            baseTrackedOffset = framingTransposer.m_TrackedObjectOffset;
            return;
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
            baseFollowOffset = transposer.m_FollowOffset;
    }

    private void Update()
    {
        if (virtualCamera == null || Time.timeScale == 0f)
            return;

        Vector2 input = ReadArrowInput();

        if (input.sqrMagnitude > 0f)
        {
            panOffset += input.normalized * panSpeed * Time.deltaTime;
            panOffset.x = Mathf.Clamp(panOffset.x, -maxPanOffset.x, maxPanOffset.x);
            panOffset.y = Mathf.Clamp(panOffset.y, -maxPanOffset.y, maxPanOffset.y);
        }
        else if (returnToCenterWhenReleased)
        {
            panOffset = Vector2.MoveTowards(panOffset, Vector2.zero, returnSpeed * Time.deltaTime);
        }

        ApplyPanOffset();
    }

    private static Vector2 ReadArrowInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            input.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow))
            input.y -= 1f;


        return input.normalized;
    }

    private void ApplyPanOffset()
    {
        Vector3 offset = new Vector3(panOffset.x, panOffset.y, 0f);

        if (usesFramingTransposer && framingTransposer != null)
        {
            framingTransposer.m_TrackedObjectOffset = baseTrackedOffset + offset;
            return;
        }

        if (transposer != null)
            transposer.m_FollowOffset = baseFollowOffset + offset;
    }
}
