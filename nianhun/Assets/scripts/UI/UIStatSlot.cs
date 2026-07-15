using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        if (statNameText != null)
            statNameText.text = statname;
    }

    private void Start()
    {
        ui = GetComponentInParent<UI>();
        UpdateStatValueUI();
    }

    private void OnEnable()
    {
        UpdateStatValueUI();
    }

    public void UpdateStatValueUI()
    {
        if (statValueText == null)
            return;

        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        PlayerStat playerStat = playermanger.instance.player.GetComponent<PlayerStat>();
        if (playerStat == null)
            return;

        statValueText.text = playerStat.GetStat(stattype).Getvalue().ToString();

        if (stattype == StatType.health)
            statValueText.text = playerStat.Getmaxhealthvalue().ToString();
        if (stattype == StatType.damage)
            statValueText.text = (playerStat.damage.Getvalue() + playerStat.strength.Getvalue()).ToString();
        if (stattype == StatType.critchance)
            statValueText.text = (playerStat.critchance.Getvalue() + playerStat.agility.Getvalue()).ToString();
        if (stattype == StatType.critpower)
            statValueText.text = (playerStat.critdamage.Getvalue() + playerStat.strength.Getvalue()).ToString();
        if (stattype == StatType.evasion)
            statValueText.text = (playerStat.evasion.Getvalue() + playerStat.agility.Getvalue()).ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui != null && ui.StatTooltip != null)
            ui.StatTooltip.ShowStatTooltip(statDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui != null && ui.StatTooltip != null)
            ui.StatTooltip.HideStatTooltip();
    }
}
