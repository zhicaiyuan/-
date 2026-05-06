using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager instance;
    private float originalTimeScale;
    private float hitStopDuration;
    private float hitStopTimer;
    private bool isHitStopping;
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
        originalTimeScale = Time.timeScale;
    }
    public void DoHitStop(float slowdownScale,float duration)
    {
        if (isHitStopping)
            return;
        Time.timeScale = slowdownScale;
        hitStopDuration = duration;
        hitStopTimer = duration;
        isHitStopping = true;
    }//抽帧函数
    private void Update()
    {
        if (isHitStopping)
        {
            hitStopTimer -= Time.unscaledDeltaTime;
            if(hitStopTimer <= 0)
            {
                Time.timeScale = originalTimeScale;
                isHitStopping = false;
            }
        }
    }
}
