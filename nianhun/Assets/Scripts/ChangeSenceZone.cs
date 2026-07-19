using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneTransitionAxis
{
    Horizontal,
    Vertical
}

[RequireComponent(typeof(Collider2D))]
public class ChangeSenceZone : MonoBehaviour
{
    [Header("目标场景")]
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] private string targetSpawnId;

    [Header("进入方向")]
    [Tooltip("Horizontal：左右门（autowalk）；Vertical：上下通道（airstate）")]
    [SerializeField] private SceneTransitionAxis transitionAxis = SceneTransitionAxis.Horizontal;

    [Header("类银恶魔城式走入")]
    [SerializeField] private Transform walkTarget;
    [SerializeField] private float walkReachDistance = 0.25f;
    [SerializeField] private float maxWalkTime = 4f;
    [SerializeField] private float transitionWalkSpeedMultiplier = 0.45f;
    [SerializeField] private float walkSlowdownDistance = 1.2f;
    [SerializeField] private float minimumWalkSpeedMultiplier = 0.2f;

    [Header("黑屏")]
    [SerializeField] private float fadeOutDuration = 1.2f;
    [SerializeField] private float blackHoldDuration = 0.2f;

    [SerializeField] private UIFadeScreen fadeScreen;

    private Collider2D zoneCollider;
    private bool isLoading;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLoading)
            return;

        Player player = collision.GetComponent<Player>();
        if (player == null || player.isbusy)
            return;

        isLoading = true;
        StartCoroutine(TransitionSequence(player));
    }

    private IEnumerator TransitionSequence(Player player)
    {
        player.isbusy = true;
        player.zerovelocity();

        UIFadeScreen screen = ResolveFadeScreen();

        Vector3 targetPosition = GetWalkTarget(player.transform.position);
        bool vertical = transitionAxis == SceneTransitionAxis.Vertical;

        float moveDirection;
        if (vertical)
        {
            moveDirection = Mathf.Sign(targetPosition.y - player.transform.position.y);
            if (moveDirection == 0f)
                moveDirection = 1f;

            player.airstate.BeginTransitionMove(moveDirection, transitionWalkSpeedMultiplier);
            player.statemachine.changestate(player.airstate);
        }
        else
        {
            moveDirection = Mathf.Sign(targetPosition.x - player.transform.position.x);
            if (moveDirection == 0f)
                moveDirection = player.facedir;

            player.autowalkstate.SetWalkDirection(moveDirection);
            player.autowalkstate.SetSpeedMultiplier(transitionWalkSpeedMultiplier);
            player.autowalkstate.SetLockVerticalVelocity(true);
            player.statemachine.changestate(player.autowalkstate);
        }

        float elapsed = 0f;
        float fadeElapsed = 0f;
        bool moveFinished = false;
        float fadeDuration = Mathf.Max(0.01f, fadeOutDuration);

        while (elapsed < maxWalkTime)
        {
            float distance = vertical
                ? Mathf.Abs(player.transform.position.y - targetPosition.y)
                : Mathf.Abs(player.transform.position.x - targetPosition.x);

            if (!moveFinished && distance <= walkReachDistance)
                moveFinished = true;

            if (!moveFinished)
            {
                float slowdown = Mathf.InverseLerp(walkReachDistance, walkSlowdownDistance, distance);
                float speedMultiplier = Mathf.Lerp(minimumWalkSpeedMultiplier, transitionWalkSpeedMultiplier, slowdown);

                if (vertical)
                    player.airstate.SetTransitionSpeedMultiplier(speedMultiplier);
                else
                    player.autowalkstate.SetSpeedMultiplier(speedMultiplier);
            }
            else
            {
                player.zerovelocity();
            }

            fadeElapsed += Time.deltaTime;
            screen?.SetAlpha(Mathf.Clamp01(fadeElapsed / fadeDuration));

            if (moveFinished && fadeElapsed >= fadeDuration)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (vertical)
            player.airstate.EndTransitionMove();

        player.zerovelocity();
        screen?.SetBlackInstant();

        if (blackHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(blackHoldDuration);

        BeginSceneLoad(screen);
    }

    private Vector3 GetWalkTarget(Vector3 fromPosition)
    {
        if (walkTarget != null)
            return walkTarget.position;

        if (zoneCollider != null)
        {
            Bounds bounds = zoneCollider.bounds;

            if (transitionAxis == SceneTransitionAxis.Vertical)
            {
                float targetY = fromPosition.y <= bounds.center.y ? bounds.max.y : bounds.min.y;
                return new Vector3(fromPosition.x, targetY, fromPosition.z);
            }

            float targetX = fromPosition.x <= bounds.center.x ? bounds.max.x : bounds.min.x;
            return new Vector3(targetX, fromPosition.y, fromPosition.z);
        }

        return transform.position;
    }

    private UIFadeScreen ResolveFadeScreen()
    {
        if (fadeScreen != null && fadeScreen.gameObject.scene.IsValid())
            return fadeScreen;

        return FindObjectOfType<UIFadeScreen>();
    }

    private void BeginSceneLoad(UIFadeScreen screen)
    {
        screen?.SetBlackInstant();
        SaveManager.instance?.SaveGame(sceneName);
        if (AudioManager.instance != null)
            AudioManager.instance.bgmIndex = 8;
        SceneTransitionData.SetTransition(targetSpawnId);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = transitionAxis == SceneTransitionAxis.Vertical ? Color.cyan : Color.yellow;

        if (walkTarget != null)
        {
            Gizmos.DrawLine(transform.position, walkTarget.position);
            Gizmos.DrawWireSphere(walkTarget.position, 0.25f);
            return;
        }

        if (zoneCollider == null)
            zoneCollider = GetComponent<Collider2D>();

        if (zoneCollider == null)
            return;

        Bounds bounds = zoneCollider.bounds;
        if (transitionAxis == SceneTransitionAxis.Vertical)
        {
            Gizmos.DrawLine(new Vector3(bounds.center.x, bounds.min.y, 0f), new Vector3(bounds.center.x, bounds.max.y, 0f));
            Gizmos.DrawWireSphere(new Vector3(bounds.center.x, bounds.max.y, 0f), 0.2f);
            Gizmos.DrawWireSphere(new Vector3(bounds.center.x, bounds.min.y, 0f), 0.2f);
        }
        else
        {
            Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.center.y, 0f), new Vector3(bounds.max.x, bounds.center.y, 0f));
            Gizmos.DrawWireSphere(new Vector3(bounds.max.x, bounds.center.y, 0f), 0.2f);
            Gizmos.DrawWireSphere(new Vector3(bounds.min.x, bounds.center.y, 0f), 0.2f);
        }
    }
}
