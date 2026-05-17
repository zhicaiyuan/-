using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIIngame : MonoBehaviour
{
    #region
    private SkillManager skills;

    private float flashcurrentCooldown = 0;
    private float dashcurrentCooldown = 0;
    private float blackholecurrentCooldown = 0;
    private float spinCurrentCooldown = 0;
    private float strikeCurrentCooldown = 0;
    private float laserCurrentCooldown = 0;
    [SerializeField] private Slider slider;
    [SerializeField] private Slider delayslider;
    [SerializeField] private PlayerStat playerstat;
    [Header("技能冷却信息")]
    [SerializeField] private Image dashcoolimage;
    [SerializeField] private Image flashcoolimage;
    [SerializeField] private Image blackholecoolimage;
    [SerializeField] private Image spincoolimage;
    [SerializeField] private Image strikecoolimage;
    [SerializeField] private Image lasercoolimage;
    [SerializeField] GameObject dashimage;
    [SerializeField] GameObject flaskimage;
    [SerializeField] GameObject blackholeimage;
    [SerializeField] GameObject spinImage;
    [SerializeField] GameObject strikeimage;
    [SerializeField] GameObject laserimage;
    private float dashCooldown;
    private float blackholeCooldown;
    private float spinCooldown;
    private float strikeCooldown;
    private float laserCooldown;
    [SerializeField] private TextMeshProUGUI flaskText;
    [SerializeField] private TextMeshProUGUI dashText;
    [SerializeField] private TextMeshProUGUI spinText;
    [SerializeField] private TextMeshProUGUI blackholeText;
    [SerializeField] private TextMeshProUGUI strikeText;
    [SerializeField] private TextMeshProUGUI laserText;

    [Header("灵魂信息")]
    [SerializeField] private TextMeshProUGUI currentSouls;
    [SerializeField] private float soulsAmount;
    [SerializeField] private float increaseRate = 200;

    #endregion
    private void Start()
    {
        skills = SkillManager.instance;
         dashCooldown = skills.dash.cooldown;
         blackholeCooldown = skills.blackhole.cooldown;
         spinCooldown = skills.spin.cooldown;
        strikeCooldown  = skills.strike.cooldown;
        laserCooldown = skills.laser.cooldown;
    }
    private void Update()
    {
        UpdateSoulsUI();

        if (playerstat != null)
            UpdateUI();
        #region 技能冷却UI组件
        if (SkillManager.instance.dash.usedskill == true)
            SetCooldownOf(dashcoolimage,dashCooldown,dashText,ref dashcurrentCooldown);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetCooldownOf(flashcoolimage,Inventory.instance.flaskCooldown,flaskText,ref flashcurrentCooldown);
        if (SkillManager.instance.blackhole.usedskill == true)
        {
            SkillManager.instance.blackhole.usedskill = false;
            SetCooldownOf(blackholecoolimage,blackholeCooldown,blackholeText,ref blackholecurrentCooldown   );
        }
        if (SkillManager.instance.spin.usedskill == true)
        {
            SkillManager.instance.spin.usedskill = false;
            SetCooldownOf(spincoolimage, spinCooldown, spinText, ref spinCurrentCooldown);
        }
        if (SkillManager.instance.strike.usedskill == true)
        {
            SkillManager.instance.strike.usedskill = false;
            SetCooldownOf(strikecoolimage, strikeCooldown, strikeText, ref strikeCurrentCooldown);
        }
        if(SkillManager.instance.laser.usedskill == true)
        {
            SkillManager.instance.laser.usedskill = false;
            SetCooldownOf(lasercoolimage, laserCooldown, laserText, ref laserCurrentCooldown);
        }
        if(SkillManager.instance.dash.dashUnlocked == false)
            dashimage.SetActive(false);
        else
            dashimage.SetActive (true);
        if (SkillManager.instance.blackhole.blackHoleUnlocked == false)
            blackholeimage.SetActive(false);
        else
            blackholeimage.SetActive(true);
        if (SkillManager.instance.spin.spinUnlocked == false)
            spinImage.SetActive(false);
        else
            spinImage.SetActive(true);
        if(SkillManager.instance.strike.strikeUnlocked == false)
            strikeimage.SetActive(false);
        else
            strikeimage.SetActive(true);
        if (SkillManager.instance.laser.laserUnlocked == false)
            laserimage.SetActive(false);
        else
            laserimage.SetActive(true);  
        

        CheckCooldownof(dashcoolimage, dashCooldown,dashText,ref dashcurrentCooldown);
        CheckCooldownof(flashcoolimage, Inventory.instance.flaskCooldown,flaskText,ref flashcurrentCooldown);
        CheckCooldownof(blackholecoolimage, blackholeCooldown,blackholeText,ref blackholecurrentCooldown);
        CheckCooldownof(spincoolimage, spinCooldown,spinText,ref spinCurrentCooldown);
        CheckCooldownof(strikecoolimage, strikeCooldown,strikeText,ref strikeCurrentCooldown);
        CheckCooldownof(lasercoolimage, laserCooldown, laserText, ref laserCurrentCooldown);
        #endregion
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
