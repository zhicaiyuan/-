using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [Header("基础设置")]
    public TextMeshProUGUI damageText;
    public float movespeed;
    public float fadeduration;
    public float scaleUpAmount;
    public float scaleUpDuartion = .2f;
    public float sideDir;

    private Color originalColor;
    private Vector2 originalPos;
    private float timer;
    private bool isInitialized;

    private CharaterStat stat;
    [SerializeField]private float riseHeight =5;
    [SerializeField]private int fallDepth =5;

    void Awake()
    {
        originalColor = damageText.color;
        originalPos = transform.localPosition;
    }

    public void Initialize(int damage,bool isCrit)
    {
        

        damageText.text = damage.ToString();

        if (isCrit)
        {
            damageText.color = Color.red;
            damageText.fontSize *= 1.2f;
            transform.localScale = Vector2.one * scaleUpAmount;

        }
        else
        {
            damageText.color = originalColor;
            transform.localScale = Vector2.one;
        }

        transform.localPosition = originalPos;
        timer = 0;
        isInitialized = true;
        sideDir = Random.Range(-0.6f, 0.6f);
    }

    void Update()
    {
        if (!isInitialized) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / Mathf.Max(fadeduration, 0.001f));

        float yOffset;
        float peakTime = 0.3f; // 30%时间到达最高点

        if (progress < peakTime)
        {
            // 上升阶段：缓动上去
            float t = progress / peakTime;
            yOffset = Mathf.Lerp(0, riseHeight, Mathf.Sin(t * Mathf.PI * 0.5f));
        }
        else
        {
            // 下降阶段：加速掉下来
            float t = (progress - peakTime) / (1f - peakTime);
            yOffset = Mathf.Lerp(riseHeight, -fallDepth, t * t); // t*t 模拟加速
        }

        // 先放大后缩小：用两段曲线或简单判断
        float scale = progress < 0.3f
            ? Mathf.Lerp(scaleUpAmount, 1f, progress / 0.3f)
            : Mathf.Lerp(1f, 0f, (progress - 0.3f) / 0.7f);
        transform.localScale = Vector3.one * scale;

        // 淡出
        damageText.alpha = Mathf.Lerp(1, 0, progress);

        if (progress >= 1f)
        {
            isInitialized = false;
            gameObject.SetActive(false); // 禁用，停止 Update
            DamageNumberPool.instance.RenturnToPool(this);
        }
    }
}

