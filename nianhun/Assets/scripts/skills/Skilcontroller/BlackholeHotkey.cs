using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlackholeHotkey : MonoBehaviour
{
    private SpriteRenderer sr;
    private KeyCode myHotKey;
    private TextMeshProUGUI myText;

    private Transform enemiesTransform;
    private BlackholeSkillcontroller blackHole;
    public void SetUpHotKey(KeyCode hotKey,Transform mytransform,BlackholeSkillcontroller myblackhole )
    {
        myText = GetComponentInChildren<TextMeshProUGUI>();
        sr = GetComponent<SpriteRenderer>();
        enemiesTransform = mytransform;
        blackHole = myblackhole;


        
        myHotKey = hotKey;
        myText.text = hotKey.ToString();
    }//设置按键复制体

    private void Update()
    {
        if (Input.GetKeyDown(myHotKey))
        {
            blackHole.AddEnemyToList(enemiesTransform);


            myText.color = Color.clear;
            sr.color = Color.clear;
        }
    }
}
