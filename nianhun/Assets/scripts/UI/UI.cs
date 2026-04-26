using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField]private DamageNumberPool pool;

    [Header("结束屏幕")]
    [SerializeField] private UIFadeScreen fadeScreen;
    public GameObject endText;
    [SerializeField] private GameObject restartButton;
    [Space]

    [SerializeField] private GameObject charcaterUI;
    [SerializeField] private GameObject skilltreeUI;
    [SerializeField] private GameObject craftUI;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject inGameUi;
    

    public UIItemTooltip ItemTooltip;
    public UIStatTooltip StatTooltip;
    public UICraftwindow craftwindow;
    // Start is called before the first frame update
    void Start()
    {
        SwitchTo(inGameUi);

        ItemTooltip.gameObject.SetActive(false);
        StatTooltip.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            SwitchWithKeyTo(charcaterUI);

        if (Input.GetKeyDown(KeyCode.B))
            SwitchWithKeyTo(craftUI);

        if (Input.GetKeyDown(KeyCode.V))
            SwitchWithKeyTo(skilltreeUI);
        if(Input.GetKeyDown(KeyCode.M))
            SwitchWithKeyTo(optionUI);
        
    }

    public void SwitchTo(GameObject menu)
    {

        for(int i = 0; i < transform.childCount; i++)
        {
            bool isFadeScreen = transform.GetChild(i).GetComponent<UIFadeScreen>() != null;//保持黑屏动画激活
            if(isFadeScreen == false)
                transform.GetChild(i).gameObject.SetActive(false);
        }//将所有子物品设置为隐藏

        if(menu != null)
        {
            menu.SetActive(true);
        }
    }//切换菜单的函数

    public void  SwitchWithKeyTo(GameObject menu)
    {
        if(menu != null && menu.activeSelf)
        {
            menu.SetActive(false);
            CheckForInGameUI();
            return;
        }

        SwitchTo(menu);
    }//快捷按键切换菜单

    private void CheckForInGameUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).gameObject.activeSelf)
                return;
        }

        SwitchTo(inGameUi);
    }//如果没有其他界面打开就打开游戏内界面


    public void SwitchOnEndScreen()
    {
        
        fadeScreen.FadeOut();
        StartCoroutine(EndScreenCorutione());

    }

    IEnumerator EndScreenCorutione()
    {
        pool.enableDamageText = false;
        yield return new WaitForSeconds(1);
        endText.SetActive(true);
        yield return new WaitForSeconds(1);
        restartButton.SetActive(true);

    }//弹出死亡文字的协程

    public void RestartGameButton() => GameManager.instance.RestartScence();
}
