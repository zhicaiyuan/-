using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 12f;

    private Vector3 defaultScale;
    private Vector3 targetScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    private void Update()
    {
        if (transform.localScale == targetScale)
            return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = defaultScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = defaultScale;
    }

    private void OnDisable()
    {
        transform.localScale = defaultScale;
        targetScale = defaultScale;
    }
}
