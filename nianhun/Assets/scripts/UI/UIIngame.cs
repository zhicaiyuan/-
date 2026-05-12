using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIngame : MonoBehaviour
{
    #region
    private skillmanager skills;

    private float flashcurrentCooldown = 0;
    private float dashcurrentCooldown = 0;
    private float blackholecurrentCooldown = 0;
    private float spinCurrentCooldown = 0;
    private float strikeCurrentCooldown = 0;
    [SerializeField] private Slider slider;
    [SerializeField] private Slider delayslider;
    [SerializeField] private PlayerStat playerstat;
    [Header("技能冷却信息")]
    [SerializeField] private Image dashimage;
    [SerializeField] private Image flaskimage;
    [SerializeField] private Image blackholeimage;
    [SerializeField] private Image spinImage;
    [SerializeField] private Image strikeImage;
    private float dashCooldown;
    private float blackholeCooldown;
    private float spinCooldown;
    private float strikeCooldown;
    [SerializeField] private TextMeshProUGUI flaskText;
    [SerializeField] private TextMeshProUGUI dashText;
    [SerializeField] private TextMeshProUGUI spinText;
    [SerializeField] private TextMeshProUGUI blackholeText;
    [SerializeField] private TextMeshProUGUI strikeText;

    [Header("灵魂信息")]
    [SerializeField] private TextMeshProUGUI currentSouls;
    [SerializeField] private float soulsAmount;
    [SerializeField] private float increaseRate = 200;

    #endregion
    private void Start()
    {
        skills = skillmanager.instance;
         dashCooldown = skills.Dash.cooldown;
         blackholeCooldown = skills.blackhole.cooldown;
         spinCooldown = skills.spin.cooldown;
        strikeCooldown  = skills.strike.cooldown;
    }
    private void Update()
    {
        UpdateSoulsUI();

        if (playerstat != null)
            UpdateUI();

        if (Input.GetKeyDown(KeyCode.Space))
            SetCooldownOf(dashimage,dashCooldown,dashText,ref dashcurrentCooldown);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetCooldownOf(flaskimage,Inventory.instance.flaskCooldown,flaskText,ref flashcurrentCooldown);
        if (skillmanager.instance.blackhole.usedskill == true)
        {
            skillmanager.instance.blackhole.usedskill = false;
            SetCooldownOf(blackholeimage,blackholeCooldown,blackholeText,ref blackholecurrentCooldown   );
        }
        if (skillmanager.instance.spin.usedskill == true)
        {
            skillmanager.instance.spin.usedskill = false;
            SetCooldownOf(spinImage, spinCooldown, spinText, ref spinCurrentCooldown);
        }
        if (skillmanager.instance.strike.usedskill == true)
        {
            skillmanager.instance.strike.usedskill = false;
            SetCooldownOf(strikeImage, strikeCooldown, strikeText, ref strikeCurrentCooldown);
        }

        CheckCooldownof(dashimage, dashCooldown,dashText,ref dashcurrentCooldown);
        CheckCooldownof(flaskimage, Inventory.instance.flaskCooldown,flaskText,ref flashcurrentCooldown);
        CheckCooldownof(blackholeimage, blackholeCooldown,blackholeText,ref blackholecurrentCooldown);
        CheckCooldownof(spinImage, spinCooldown,spinText,ref spinCurrentCooldown);
        CheckCooldownof(strikeImage, strikeCooldown,strikeText,ref strikeCurrentCooldown);
    }

    private void UpdateSoulsUI()
    {
        if (soulsAmount < playermanger.instance.CurrentCurrencyAmount())
            soulsAmount += Time.deltaTime * increaseRate;
        else
            soulsAmount = playermanger.instance.CurrentCurrencyAmount();

        currentSouls.text = ((int)soulsAmount).ToString();
    }//更新灵魂

    private void UpdateText(TextMeshProUGUI text,float cooldown)
    {
        float cooldowntext = cooldown;
        text.text = cooldowntext.ToString();
    }
    private void UpdateUI()
    {
        slider.maxValue = playerstat.Getmaxhealthvalue();
        slider.value = playerstat.currenthealth;
        delayslider.maxValue = playerstat.Getmaxhealthvalue();
        delayslider.value = Mathf.Lerp(delayslider.value, slider.value, Time.deltaTime * 2f);
    }//更新生命值

    private void SetCooldownOf(Image image,float cooldown,TextMeshProUGUI text,ref float currentCooldown)
    {
        if (image.fillAmount <= 0)
        {
        image.fillAmount = 1;
        text.enabled = true;
        currentCooldown = cooldown;
        text.text = cooldown.ToString();

        }
            
        
    }

    private void CheckCooldownof(Image image,float cooldown,TextMeshProUGUI text,ref float currentCooldown)
    {
        if(image.fillAmount > 0)
        {
            image.fillAmount -= 1/cooldown * Time.deltaTime;
            currentCooldown -= Time.deltaTime;
            text.text = currentCooldown.ToString("0.0");
        }
        if(image.fillAmount == 0)
        {
            text.enabled = false;
        }
    }
}
