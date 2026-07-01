using TMPro;
using UnityEngine;

public class UIPopUpTextManager : MonoBehaviour
{
    public static UIPopUpTextManager instance;

    [SerializeField] private GameObject popUpTextPrefab;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void SetPrefab(GameObject prefab)
    {
        popUpTextPrefab = prefab;
    }

    public void Show(string text)
    {
        if (popUpTextPrefab == null)
        {
            Debug.LogWarning("UIPopUpTextManager: popUpTextPrefab is not assigned.");
            return;
        }

        transform.SetAsLastSibling();

        GameObject newText = Instantiate(popUpTextPrefab, transform);
        RectTransform rectTransform = newText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        TMP_Text tmpText = newText.GetComponent<TMP_Text>();
        tmpText.text = text;
        Color color = tmpText.color;
        tmpText.color = new Color(color.r, color.g, color.b, 1f);
    }
}
