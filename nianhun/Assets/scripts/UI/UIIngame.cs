using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIngame : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Slider delayslider;
    [SerializeField] private PlayerStat playerstat;

    [SerializeField] private Image dashimage;
    [SerializeField] private float dashCooldown;
    private void Start()
    {
        
    }
    private void Update()
    {
        if(playerstat != null)
            UpdateUI();

        if(Input.GetKeyDown(KeyCode.Space))
            SetCooldownOf(dashimage);

        CheckCooldownof(dashimage,dashCooldown);
    }
    private void UpdateUI()
    {
        slider.maxValue = playerstat.Getmaxhealthvalue();
        slider.value = playerstat.currenthealth;
        delayslider.maxValue = playerstat.Getmaxhealthvalue();
        delayslider.value = Mathf.Lerp(delayslider.value, slider.value, Time.deltaTime * 2f);
    }//更新生命值

    private void SetCooldownOf(Image image)
    {
        if (image.fillAmount <= 0)
            image.fillAmount = 1;
        
    }

    private void CheckCooldownof(Image image,float cooldown)
    {
        if(image.fillAmount > 0)
            image.fillAmount -= 1/cooldown * Time.deltaTime;
    }
}
