using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMemu : MonoBehaviour
{
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] private GameObject continueButton;
    [SerializeField] UIFadeScreen fadeScreen;

    private void Start()
    {
        AudioManager.instance.bgmIndex = 0;
        if (SaveManager.instance.HasSaveData() == false)
        {
            continueButton.SetActive(false);
        }
    }
    public void ContinueGame()
    {
        StartCoroutine(LoadSenceWithFadeEffect(1.5f));
    }

    public void NewGame()
    {
        SaveManager.instance.DeleteSaveData();
        StartCoroutine(LoadSenceWithFadeEffect(1.5f));
    }

    public void ExitGame()
    {
        Debug.Log("离开游戏");
        //Application.Quit();
    }

    IEnumerator LoadSenceWithFadeEffect(float delay)
    {
        fadeScreen.FadeOut();

        yield return new WaitForSeconds(delay);

        AudioManager.instance.bgmIndex = 8;
        SceneManager.LoadScene(sceneName);
    }
}
