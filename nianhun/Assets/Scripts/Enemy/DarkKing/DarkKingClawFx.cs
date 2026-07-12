using UnityEngine;

public class DarkKingClawFx : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float lifeTime = 0.8f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator != null)
            animator.Play("claw", 0, 0f);

        Destroy(gameObject, lifeTime);
    }
}
