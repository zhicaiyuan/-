using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI : MonoBehaviour,ISaveManager
{
    private const string PopUpTextPrefabPath = "Assets/Prefabs/UI/UIPopUpText.prefab";

    [SerializeField]private DamageNumberPool pool;
    [SerializeField] private GameObject uiPopUpTextPrefab;

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

    [SerializeField] private UIVolumeSlider[] volumeSettings;
    private void Awake()
    {
        fadeScreen.gameObject.SetActive(true);
        EnsurePopUpOverlay();
    }
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
            if (!ShouldKeepChildActive(transform.GetChild(i)))
                transform.GetChild(i).gameObject.SetActive(false);
        }//将所有子物品设置为隐藏

        if(menu != null)
        {
            if (menu != inGameUi)
            {
                fadeScreen.CancelFadeAndClear();
                menu.transform.SetAsLastSibling();
            }

            AudioManager.instance.PlaySFX(14, null);
            menu.SetActive(true);
        }

        if(GameManager.instance != null)
        {
            if (menu == inGameUi)
            {
                fadeScreen.ResumeFading();
                GameManager.instance.PauseGame(false);
            }
            else
                GameManager.instance.PauseGame(true);
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
        AudioManager.instance.PlaySFX(14, null);
        SwitchTo(menu);
    }//快捷按键切换菜单

    private void CheckForInGameUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf && !ShouldKeepChildActive(child))
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

    public void RestartGameButton()
    {
        AudioManager.instance.PlaySFX(14, null);
        GameManager.instance.RestartScence();
    }

    public void LoadData(GameData data)
    {
        foreach(KeyValuePair<string,float> pair in data.volumeSettings)
        {
            foreach(UIVolumeSlider slider in volumeSettings)
            {
                if(slider.parametr == pair.Key)
                    slider.LoadSlider(pair.Value);
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        data.volumeSettings.Clear();
        foreach(UIVolumeSlider item in volumeSettings)
        {
            data.volumeSettings.Add(item.parametr, item.slider.value);
        }
    }//保存设置

    private bool ShouldKeepChildActive(Transform child)
    {
        return child.GetComponent<UIFadeScreen>() != null
            || child.GetComponent<UIPopUpTextManager>() != null;
    }

    private void EnsurePopUpOverlay()
    {
        UIPopUpTextManager existingManager = GetComponentInChildren<UIPopUpTextManager>(true);
        if (existingManager != null)
        {
            GameObject prefab = GetPopUpTextPrefab();
            if (prefab != null)
                existingManager.SetPrefab(prefab);
            existingManager.gameObject.SetActive(true);
            return;
        }

        GameObject overlayGo = new GameObject("PopUpTextOverlay", typeof(RectTransform));
        overlayGo.transform.SetParent(transform, false);

        RectTransform overlayRect = overlayGo.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;

        UIPopUpTextManager manager = overlayGo.AddComponent<UIPopUpTextManager>();
        GameObject popUpPrefab = GetPopUpTextPrefab();
        if (popUpPrefab != null)
            manager.SetPrefab(popUpPrefab);

        overlayGo.SetActive(true);
    }

    private GameObject GetPopUpTextPrefab()
    {
        if (uiPopUpTextPrefab != null)
            return uiPopUpTextPrefab;

        uiPopUpTextPrefab = Resources.Load<GameObject>("UIPopUpText");

#if UNITY_EDITOR
        if (uiPopUpTextPrefab == null)
            uiPopUpTextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PopUpTextPrefabPath);
#endif

        return uiPopUpTextPrefab;
    }
}
