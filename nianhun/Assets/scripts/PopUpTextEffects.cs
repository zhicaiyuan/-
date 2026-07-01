using TMPro;
using UnityEngine;

public class PopUpTextEffects : MonoBehaviour
{
    private TMP_Text myText;
    private RectTransform rectTransform;
    private bool useRectTransform;

    [SerializeField] private float speed;
    [SerializeField] private float disappearingspeed;
    [SerializeField] private float colordisappearingspeed;

    [SerializeField] private float lifeTime;

    private float textTimer;

    private void Start()
    {
        myText = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
        useRectTransform = rectTransform != null;
        textTimer = lifeTime;
    }

    private void Update()
    {
        float moveDelta = speed * Time.unscaledDeltaTime;

        if (useRectTransform)
            rectTransform.anchoredPosition += Vector2.up * moveDelta;
        else
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y + 1), moveDelta);

        textTimer -= Time.unscaledDeltaTime;

        if (textTimer <= 0)
        {
            float alpha = myText.color.a - colordisappearingspeed * Time.unscaledDeltaTime;
            myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, alpha);

            if (myText.color.a < 0.5f)
                speed = disappearingspeed;

            if (myText.color.a <= 0)
                Destroy(gameObject);
        }
    }
}
