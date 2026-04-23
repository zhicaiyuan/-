using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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
    private float targetScale;

    void Awake()
    {
        originalColor = damageText.color;
        
    }

    public void Initialize(int damage,bool isCrit,bool isavoid)
    {
        targetScale = 1.0f;

        damageText.text = damage.ToString();
        damageText.color = Color.white;


        if (isCrit)
        {
            damageText.color = Color.red;
            damageText.text = "暴击 " + damage.ToString();
        }//设置暴击样式
        else if (isavoid)
        {
            damageText.text = "闪避";
        }//设置闪避样式
        else
        {
            damageText.color = originalColor;

        }

        originalPos = transform.localPosition;//重置位置让文字在对象位置飘出
        timer = 0;
        isInitialized = true;
        sideDir = Random.Range(-10f, 10f);
    }

    void Update()
    {
        if (!isInitialized) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / Mathf.Max(fadeduration, 0.001f));

        float yOffset;
        float peakTime = 0.3f; // 30%时间到达最高点
        float scale;

        if (progress < peakTime)
        {
            // 上升阶段：缓动上去
            float t = progress / peakTime;
            yOffset = Mathf.Lerp(0, riseHeight, Mathf.Sin(t * Mathf.PI * 0.5f));
            scale = Mathf.Lerp(1f, targetScale, progress / 0.3f);
        }
        else
        {
            // 下降阶段：加速掉下来
            float t = (progress - peakTime) / (1f - peakTime);
            yOffset = Mathf.Lerp(riseHeight, -fallDepth, t * t); // t*t 模拟加速
            scale = Mathf.Lerp(targetScale, 1f, (progress - 0.3f) / 0.7f);
        }

        // 先放大后缩小：用两段曲线或简单判断
        transform.localPosition = originalPos + new Vector2(sideDir * 20f * progress,yOffset);
        damageText.fontSize = damageText.fontSize
            * scale;

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

