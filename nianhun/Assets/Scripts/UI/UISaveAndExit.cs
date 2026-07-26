using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISaveAndExit : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "主菜单";
    [SerializeField] private UIFadeScreen fadeScreen;
    [SerializeField] private float fadeDuration = 1.2f;

    private enum SaveAndExitDestination
    {
        MainMenu,
        Desktop
    }

    public void SaveAndExitToMainMenu()
    {
        PrepareExit();
        StartCoroutine(SaveAndExitRoutine(SaveAndExitDestination.MainMenu));
    }

    public void SaveAndExitToDesktop()
    {
        PrepareExit();
        StartCoroutine(SaveAndExitRoutine(SaveAndExitDestination.Desktop));
    }

    private void PrepareExit()
    {
        if (GameManager.instance != null)
            GameManager.instance.PauseGame(false);

        AudioManager.instance?.PlaySFX(14, null);
    }

    private IEnumerator SaveAndExitRoutine(SaveAndExitDestination destination)
    {
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();

        if (fadeScreen != null)
        {
            fadeScreen.FadeOut(fadeDuration);
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        if (destination == SaveAndExitDestination.MainMenu)
        {
            EditorPlayModeHelpers.ClearSelectionBeforeSceneLoad();
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
            QuitApplication();
    }

    private static void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Reset()
    {
        if (fadeScreen == null)
            fadeScreen = FindObjectOfType<UIFadeScreen>();
    }
}
