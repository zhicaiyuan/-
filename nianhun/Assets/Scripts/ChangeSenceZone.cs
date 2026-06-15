using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ChangeSenceZone : MonoBehaviour
{
    [Header("目标场景")]
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] private string targetSpawnId;

    [Header("自动走入")]
    [SerializeField] private Transform walkTarget;
    [SerializeField] private float walkReachDistance = 0.35f;
    [SerializeField] private float maxWalkTime = 5f;

    [Header("黑屏")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float blackHoldDuration = 0.15f;

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
        if (player == null)
            return;

        isLoading = true;
        StartCoroutine(TransitionSequence(player));
    }

    private IEnumerator TransitionSequence(Player player)
    {
        player.isbusy = true;

        UIFadeScreen screen = ResolveFadeScreen();
        screen?.FadeOut();

        Vector3 targetPosition = GetWalkTarget(player.transform.position);
        float walkDirection = Mathf.Sign(targetPosition.x - player.transform.position.x);
        if (walkDirection == 0f)
            walkDirection = player.facedir;

        player.autowalkstate.SetWalkDirection(walkDirection);
        player.statemachine.changestate(player.autowalkstate);

        float elapsed = 0f;
        while (elapsed < maxWalkTime)
        {
            float distance = Vector2.Distance(player.transform.position, targetPosition);
            if (distance <= walkReachDistance)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.zerovelocity();

        float remainingFade = fadeOutDuration - elapsed;
        if (remainingFade > 0f)
            yield return new WaitForSeconds(remainingFade);

        if (blackHoldDuration > 0f)
            yield return new WaitForSeconds(blackHoldDuration);

        BeginSceneLoad();
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

    private void BeginSceneLoad()
    {
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
