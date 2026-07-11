using UnityEngine;

public class TimeKingThunderFx : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 16f;
    [SerializeField] private float lifeTime = 0.6f;

    private float elapsed;
    private int frameIndex;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (frames != null && frames.Length > 0 && spriteRenderer != null)
        {
            int next = Mathf.FloorToInt(elapsed * frameRate);
            if (next != frameIndex && next < frames.Length)
            {
                frameIndex = next;
                spriteRenderer.sprite = frames[frameIndex];
            }
        }

        if (elapsed >= lifeTime)
            Destroy(gameObject);
    }

    public void Play(Sprite[] thunderFrames, float rate = 16f, float duration = 0.6f)
    {
        frames = thunderFrames;
        frameRate = rate;
        lifeTime = duration;
        elapsed = 0f;
        frameIndex = 0;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && frames != null && frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }
}
