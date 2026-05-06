using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlackholeSkillcontroller : MonoBehaviour
{
    [SerializeField] private GameObject hotkeyPerfab;
    [SerializeField] private List<KeyCode> keyCodeList;

    private float maxSize;
    private float growSpeed;
    private float shrinkSpeed;
    private float blackHoleTimer;

    private bool canGrow = true;
    private bool canShrink;

    private bool canCreateHotKey = true;
    private bool cloneAttackReleased;
    private int amountOfAttacks = 4;
    private float cloneAttackCooldown = .3f;
    private float cloneAttackTimer;

    private List<Transform> target = new List<Transform>();
    private List<GameObject> createdHotKey = new List<GameObject>();

    public bool playerCanExitState {  get; private set; }

    public void SetUpBlackHole(float maxsize,float growspped,float shrinkspeed,int amountofattack,float cloneattackcooldown,float balckholeDuration)
    {
        maxSize = maxsize; growSpeed = growspped; shrinkSpeed = shrinkspeed; amountOfAttacks = amountofattack; cloneAttackCooldown = cloneattackcooldown;blackHoleTimer = balckholeDuration;
    }
    private void Update()
    {
        cloneAttackTimer -= Time.deltaTime;
        blackHoleTimer -= Time.deltaTime;

        if(blackHoleTimer < 0)
        {
            blackHoleTimer = Mathf.Infinity;

            if (target.Count > 0)
                RelaseCloneAttack();
            else
                FinishBlackHoleAbility();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            RelaseCloneAttack();
        }

        CloneAttackLLogic();

        if (canGrow && !canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), growSpeed * Time.deltaTime);
        }
        if (canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(-1, -1), shrinkSpeed * Time.deltaTime);
            if (transform.localScale.x < 0)
                Destroy(gameObject);
        }
    }

    private void RelaseCloneAttack()
    {
        if (target.Count <= 0)
            return;

        DestoryHotKeys();
        cloneAttackReleased = true;
        canCreateHotKey = false;
        playermanger.instance.player.MakeTransprent(true);
    }

    private void CloneAttackLLogic()
    {
        if (cloneAttackTimer < 0 && cloneAttackReleased)
        {
            cloneAttackTimer = cloneAttackCooldown;
            int ramdomIndex = Random.Range(0, target.Count);
            float xOffset;
            if (Random.Range(0, 100) > 50)
                xOffset = .5f;
            else
                xOffset = -.5f;
            skillmanager.instance.clone.CreateClone(target[ramdomIndex], new Vector3(xOffset, 0));
            amountOfAttacks--;

            if (amountOfAttacks <= 0)
            {
                Invoke("FinishBlackHoleAbility",1f);
            }
        }
    }

    private void FinishBlackHoleAbility()
    {
        DestoryHotKeys();
        playerCanExitState = true;
        canShrink = true;
        cloneAttackReleased = false;

    }

    private void DestoryHotKeys()
    {
        if (createdHotKey.Count <= 0)
            return;
        for (int i = 0; i < createdHotKey.Count; i++)
        {
            Destroy(createdHotKey[i]);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>()!= null)
        {
            collision.GetComponent<Enemy>().freezeTime(true);
            CreateHotKey(collision);
            playermanger.instance.player.Stat.DoFixedDamage(collision.GetComponent<enemystat>(),20);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>()!= null)
            collision.GetComponent<Enemy>().freezeTime(false);
    }

    private void CreateHotKey(Collider2D collision)
    {
        if (keyCodeList.Count <= 0)
            return;
        if (!canCreateHotKey)
            return;

        GameObject newHotKey = Instantiate(hotkeyPerfab, collision.transform.position + new Vector3(0, 2), Quaternion.identity);
        createdHotKey.Add(newHotKey);

        KeyCode choosenKey = keyCodeList[Random.Range(0, keyCodeList.Count)];
        keyCodeList.Remove(choosenKey);

        BlackholeHotkey newHotKeyScript = newHotKey.GetComponent<BlackholeHotkey>();

        newHotKeyScript.SetUpHotKey(choosenKey,collision.transform,this);
        
    }//生成对应按键

    public void AddEnemyToList(Transform enemyTransform) => target.Add(enemyTransform);
}
