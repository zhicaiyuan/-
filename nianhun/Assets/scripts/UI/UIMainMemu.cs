using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMemu : MonoBehaviour
{
    [SerializeField] private string sceneName = "主场景";
    [SerializeField] private GameObject continueButton;

    private void Start()
    {
        if (SaveManager.instance.HasSaveData() == false)
        {
            continueButton.SetActive(false);
        }
    }
    public void ContinueGame()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame()
    {
        SaveManager.instance.DeleteSaveData();
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Debug.Log("离开游戏");
        //Application.Quit();
    }
}
