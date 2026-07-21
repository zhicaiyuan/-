using UnityEngine;

/// <summary>
/// 拉杆：靠近后按 F 扳动，打开关联的门，并把激活状态写入存档。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LeverSwitch : MonoBehaviour, ISaveManager
{
    [Header("存档")]
    [Tooltip("默认留空，运行时按「场景名_坐标」自动生成。")]
    [SerializeField] private string id;
    [SerializeField] private bool useManualId;

    [Header("表现")]
    [SerializeField] private Animator animator;
    [SerializeField] private string activeBoolName = "Active";
    [SerializeField] private string pullStateName = "Lever";
    [SerializeField] private int sfxIndex = 34;

    [Header("关联的门")]
    [SerializeField] private LeverDoor[] doors;

    private bool isActivated;
    private Collider2D triggerCollider;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        EnsureStableId();
    }

    private void Reset()
    {
        animator = GetComponent<Animator>();
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (box.size == Vector2.one || box.size == Vector2.zero)
            box.size = new Vector2(1.2f, 1.2f);
    }

    [ContextMenu("生成手动拉杆id")]
    private void GenerateManualId()
    {
        useManualId = true;
        id = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("改为自动坐标id")]
    private void UseAutoId()
    {
        useManualId = false;
        id = string.Empty;
        EnsureStableId();
    }

    public void EnsureStableId()
    {
        if (useManualId && !string.IsNullOrEmpty(id))
            return;

        if (!gameObject.scene.IsValid() || string.IsNullOrEmpty(gameObject.scene.name))
        {
            id = string.Empty;
            return;
        }

        id = $"{gameObject.scene.name}_lever_{transform.position.x:F1}_{transform.position.y:F1}";
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isActivated)
            return;

        if (collision.GetComponent<Player>() == null)
            return;

        if (Input.GetKeyDown(KeyCode.F))
            Activate(instant: false);
    }

    public void Activate(bool instant)
    {
        if (isActivated)
            return;

        EnsureStableId();
        isActivated = true;
        PlayLeverVisual(instant);
        OpenDoors(instant);

        if (!instant)
        {
            if (AudioManager.instance != null && sfxIndex >= 0)
                AudioManager.instance.PlaySFX(sfxIndex, transform);
            SaveManager.instance?.SaveGame();
        }
    }

    private void PlayLeverVisual(bool instant)
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(activeBoolName) && HasAnimatorBool(activeBoolName))
            animator.SetBool(activeBoolName, true);

        if (string.IsNullOrEmpty(pullStateName))
            return;

        if (instant)
        {
            animator.Play(pullStateName, 0, 1f);
            animator.Update(0f);
        }
        else
        {
            animator.Play(pullStateName, 0, 0f);
        }
    }

    private void OpenDoors(bool instant)
    {
        if (doors == null)
            return;

        foreach (LeverDoor door in doors)
        {
            if (door != null)
                door.Open(instant);
        }
    }

    private bool HasAnimatorBool(string paramName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == paramName)
                return true;
        }

        return false;
    }

    public void LoadData(GameData data)
    {
        EnsureStableId();

        if (string.IsNullOrEmpty(id) || data.activatedLevers == null)
            return;

        if (data.activatedLevers.TryGetValue(id, out bool activated) && activated)
            Activate(instant: true);
    }

    public void SaveData(ref GameData data)
    {
        EnsureStableId();

        if (string.IsNullOrEmpty(id) || !isActivated)
            return;

        if (data.activatedLevers == null)
            data.activatedLevers = new SerializableDictionary<string, bool>();

        data.activatedLevers[id] = true;
    }
}
