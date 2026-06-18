using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ChangeSenceZone : MonoBehaviour
{
    [Header("目标场景")]
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] private string targetSpawnId;

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
        float walkDirection = Mathf.Sign(targetPosition.x - player.transform.position.x);
        if (walkDirection == 0f)
            walkDirection = player.facedir;

        player.autowalkstate.SetWalkDirection(walkDirection);
        player.autowalkstate.SetSpeedMultiplier(transitionWalkSpeedMultiplier);
        player.autowalkstate.SetLockVerticalVelocity(true);
        player.statemachine.changestate(player.autowalkstate);

        float elapsed = 0f;
        float fadeElapsed = 0f;
        bool walkFinished = false;
        float fadeDuration = Mathf.Max(0.01f, fadeOutDuration);

        while (elapsed < maxWalkTime)
        {
            float distance = Mathf.Abs(player.transform.position.x - targetPosition.x);
            if (!walkFinished && distance <= walkReachDistance)
                walkFinished = true;

            if (!walkFinished)
            {
                float slowdown = Mathf.InverseLerp(walkReachDistance, walkSlowdownDistance, distance);
                float speedMultiplier = Mathf.Lerp(minimumWalkSpeedMultiplier, transitionWalkSpeedMultiplier, slowdown);
                player.autowalkstate.SetSpeedMultiplier(speedMultiplier);
            }
            else
            {
                player.zerovelocity();
            }

            fadeElapsed += Time.deltaTime;
            screen?.SetAlpha(Mathf.Clamp01(fadeElapsed / fadeDuration));

            if (walkFinished && fadeElapsed >= fadeDuration)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

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
        SaveManager.instance?.SaveGame();
        AudioManager.instance.bgmIndex = 8;
        SceneTransitionData.SetTransition(targetSpawnId);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDrawGizmosSelected()
    {
        if (walkTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, walkTarget.position);
            Gizmos.DrawWireSphere(walkTarget.position, 0.25f);
        }
    }
}
