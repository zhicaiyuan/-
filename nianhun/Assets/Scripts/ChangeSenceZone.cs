using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSenceZone : MonoBehaviour
{
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] UIFadeScreen fadeScreen;
    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLoading) return;
        if (collision.GetComponent<Player>() != null)
        {
            isLoading = true;
            StartCoroutine(LoadSceneWithFadeEffect(.5f));
        }
    }
    IEnumerator LoadSceneWithFadeEffect(float delay)
    {
        if (fadeScreen == null)
        {
            fadeScreen = FindObjectOfType<UIFadeScreen>();
            if (fadeScreen == null)
            {
                Debug.LogWarning("UIFadeScreen 未赋值且场景中未找到。将直接切换场景。");
                yield return new WaitForSeconds(delay);
                AudioManager.instance.bgmIndex = 8;
                SceneManager.LoadScene(sceneName);
                yield break;
            }
        }

        fadeScreen.FadeOut();

        yield return new WaitForSeconds(delay);

        AudioManager.instance.bgmIndex = 8;
        SceneManager.LoadScene(sceneName);
    }
}
