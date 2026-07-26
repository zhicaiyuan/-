using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMemu : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "森林苏醒之地";
    [SerializeField] private GameObject continueButton;
    [SerializeField] UIFadeScreen fadeScreen;

    private void Start()
    {
        AudioManager.instance.bgmIndex = 0;
        if (SaveManager.instance.HasSaveData() == false)
            continueButton.SetActive(false);
    }

    public void ContinueGame()
    {
        AudioManager.instance.PlaySFX(14, null);
        string sceneToLoad = SaveManager.instance.GetContinueSceneName(newGameSceneName);
        StartCoroutine(LoadSenceWithFadeEffect(sceneToLoad, 1.5f));
    }

    public void NewGame()
    {
        AudioManager.instance.PlaySFX(14, null);
        SaveManager.instance.DeleteSaveData();
        StartCoroutine(LoadSenceWithFadeEffect(newGameSceneName, 1.5f));
    }

    public void ExitGame()
    {
        AudioManager.instance.PlaySFX(14, null);
        Debug.Log("离开游戏");
        //Application.Quit();
    }

    IEnumerator LoadSenceWithFadeEffect(string sceneName, float delay)
    {
        fadeScreen.FadeOut();

        yield return new WaitForSeconds(delay);

        AudioManager.instance.bgmIndex = 8;
        EditorPlayModeHelpers.ClearSelectionBeforeSceneLoad();
        SceneManager.LoadScene(sceneName);
    }
}
