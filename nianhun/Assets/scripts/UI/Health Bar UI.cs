using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private Entity entity;
    private CharaterStat mystat;
    private RectTransform myTransform;
    [SerializeField] private Slider slider;
    [SerializeField] Slider delayslider;
    private GameObject fire;
    private GameObject chill;
    private new GameObject light;

    private void Start()
    {
        myTransform = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        mystat = GetComponentInParent<CharaterStat>();
        fire = transform.Find("fire").gameObject;
        chill = transform.Find("chill").gameObject;
        light = transform.Find("light").gameObject;
        //获取组件

        entity.onfilped += FlipUI;//用时间触发函数
        mystat.onhealthchanged += UpdateUI;

        UpdateUI();
        showicon();
    }

    private void Update()
    {
        UpdateUI();
        showicon();
    }
    private void FlipUI()
    {
        myTransform.Rotate(0, 180, 0);//旋转
    }

    private void UpdateUI()
    {
        slider.maxValue = mystat.Getmaxhealthvalue();
        slider.value =mystat.currenthealth;
        delayslider.maxValue = mystat.Getmaxhealthvalue();
        delayslider.value = Mathf.Lerp(delayslider.value,slider.value,Time.deltaTime * 2f);
    }//更新生命值

    private void OnDisable()
    {
        entity.onfilped -= FlipUI;
        mystat.onhealthchanged -= UpdateUI;//减去防止叠加
    }
    private void showicon()
    {
        if(fire == null || chill == null || light == null)
            return;

        if (mystat.isfired)
            fire.SetActive(true);
        else
            fire.SetActive(false);
        if (mystat.ischilled)
            chill.SetActive(true);
        else
            chill.SetActive(false);
        if (mystat.isshocked)
            light.SetActive(true);
        else
            light.SetActive(false);
        
    }

}
