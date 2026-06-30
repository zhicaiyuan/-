using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISaveAndExit : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "主菜单";
    [SerializeField] private UIFadeScreen fadeScreen;
    [SerializeField] private float fadeDuration = 1.2f;

    public void SaveAndExitToMainMenu()
    {
        if (GameManager.instance != null)
            GameManager.instance.PauseGame(false);

        AudioManager.instance?.PlaySFX(14, null);
        StartCoroutine(SaveAndExitRoutine());
    }

    private IEnumerator SaveAndExitRoutine()
    {
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();

        if (fadeScreen != null)
        {
            fadeScreen.FadeOut(fadeDuration);
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void Reset()
    {
        if (fadeScreen == null)
            fadeScreen = FindObjectOfType<UIFadeScreen>();
    }
}
