using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIngame : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Slider delayslider;
    [SerializeField] private PlayerStat playerstat;

    [SerializeField] private Image dashimage;
    [SerializeField] private Image flaskimage;
    [SerializeField] private float dashCooldown;

    [SerializeField] private TextMeshProUGUI currentSouls;
    private void Start()
    {
        
    }
    private void Update()
    {
        currentSouls.text = playermanger.instance.CurrentCurrencyAmount().ToString();

        if(playerstat != null)
            UpdateUI();

        if(Input.GetKeyDown(KeyCode.Space))
            SetCooldownOf(dashimage);
        if(Input.GetKeyDown(KeyCode.Alpha1))
            SetCooldownOf(flaskimage);


        CheckCooldownof(dashimage,dashCooldown);
        CheckCooldownof(flaskimage,Inventory.instance.flaskCooldown);
        
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
