using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStatSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private UI ui;

    [SerializeField] private string statname;
    [SerializeField] private StatType stattype;
    [SerializeField] private TextMeshProUGUI statValueText;
    [SerializeField] private TextMeshProUGUI statNameText;

    [TextArea]
    [SerializeField] private string statDescription;
    private void OnValidate()
    {
        gameObject.name = "属性：" + statname;


        if(statValueText != null )
            statNameText.text = statname;
    }
     void Start()
    {
        UpdateStatValueUI();

        ui = GetComponentInParent<UI>();
    }
    public void UpdateStatValueUI()
    {
        PlayerStat playerStat = playermanger.instance.player.GetComponent<PlayerStat>();

        if (playerStat != null)
        {
            statValueText.text = playerStat.GetStat(stattype).Getvalue().ToString();

            if(stattype == StatType.health)
                statValueText.text = playerStat.Getmaxhealthvalue().ToString();//更新血量防止出错下同
            if(stattype == StatType.damage)
                statValueText.text = (playerStat.damage.Getvalue() + playerStat.strength.Getvalue()).ToString();
            if(stattype == StatType.critchance)
                statValueText.text = (playerStat.critchance.Getvalue()+playerStat.agility.Getvalue()).ToString();
            if(stattype == StatType.critpower)
                statValueText.text = (playerStat.critdamage.Getvalue()+playerStat.strength.Getvalue()).ToString();
            if(stattype == StatType.evasion)
                statValueText.text = (playerStat.evasion.Getvalue()+ playerStat.agility.Getvalue()).ToString();
        }
    }//更新UI属性值

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.StatTooltip.ShowStatTooltip(statDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.StatTooltip.HideStatTooltip();
    }
}
