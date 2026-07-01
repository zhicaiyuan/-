using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

public class EntityFx : MonoBehaviour
{
    private Player player;
    private SpriteRenderer sr;
    [Header("flashfx")]
    [SerializeField] private Material Hitmat;
     private Material originalmat;

    [Header("Screen Shake")]
     private CinemachineImpulseSource screenshake;
    [SerializeField] private float shakeMultiplier;
    [SerializeField] private Vector3 shakePower;

    [Header("PopUp Text")]
    [SerializeField] private GameObject popUpTextPerfab;

    [Header("Ailment colors")]
    [SerializeField] private Color[] chillcolor;
    [SerializeField] private Color[] firecolor;
    [SerializeField] private Color[] shockColor;

    [Header("HitFx")]
    [SerializeField] private GameObject hitFx;
    [SerializeField] private GameObject criticalHitFx;
    [SerializeField] private GameObject focusFx;
    [SerializeField] private GameObject smokeFx;
    [SerializeField] private GameObject attack1Fx;
    [SerializeField] private GameObject attack2Fx;
    [SerializeField] private GameObject attack3Fx;
    private Coroutine attackFxRoutine;

    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        player = playermanger.instance.player;
        screenshake = GetComponent<CinemachineImpulseSource>();
        originalmat = sr.material;

    }

    public void ScreenShake()
    {
        screenshake.m_DefaultVelocity = new Vector3(shakePower.x * player.facedir,shakePower.y) * shakeMultiplier;
        screenshake.GenerateImpulse();
    }

    private IEnumerator flashfx()
    {
        sr.material = Hitmat;
        Color currentcolor = sr.color;

        sr.color = Color.white;

        yield return new WaitForSeconds(0.2f);

        sr.color = currentcolor;
        sr.material = originalmat;
    }

    private void redcolourblink()
    {
        if (sr.color != Color.red)
            sr.color = Color.red;
        else
            sr.color = Color.white;
    }

    public void CreatePopUpText(string text)
    {
        if (UIPopUpTextManager.instance != null)
        {
            UIPopUpTextManager.instance.Show(text);
            return;
        }

        if (popUpTextPerfab == null || Camera.main == null)
            return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        GameObject newText = Instantiate(popUpTextPerfab, worldPos, Quaternion.identity);
        TMP_Text tmpText = newText.GetComponent<TMP_Text>();
        if (tmpText != null)
            tmpText.text = text;
    }
    private void cancelcolorchange()
    {
        CancelInvoke();
        sr.color= Color.white;
    }//取消颜色变换
    public void InvokeFireFxFor(float seconds)
    {
        CancelInvoke();
        InvokeRepeating("fireColorFx", 0, 1);//等待时间切换
        Invoke("cancelcolorchange",seconds);
    }//火

    public void InvokeChillFxFor(float seconds)
    {
        CancelInvoke();
        InvokeRepeating("chillColorFx", 0, 0.3f);//等待时间切换
        Invoke("cancelcolorchange", seconds);//持续变蓝
    }//冰

    public void InvokeShockFxFor(float seconds)
    {
        CancelInvoke();
        InvokeRepeating("shockColorFx", 0, 0.3f);//等待时间切换
        Invoke("cancelcolorchange", seconds);
    }//雷

    private void fireColorFx()
    {
        if (sr.color != firecolor[0])
            sr.color = firecolor[0];
        else
            sr.color = firecolor[1];//来回切换
        
    }//火焰元素颜色

    private void chillColorFx()
    {
        if (sr.color != chillcolor[0])
            sr.color = chillcolor[0];
        else
            sr.color = chillcolor[1];
    }//寒冰元素颜色

    private void shockColorFx()
    {
        if (sr.color != shockColor[0])
            sr.color = shockColor[0];
        else
            sr.color = shockColor[1];
    }//雷元素颜色

    public void CreateHitFx(Transform target,bool critical)
    {
        
        float zRotation =Random.Range(-90,90);//随机角度
        float xPosition = Random.Range(-.5f, .5f); 
        float yPosition = Random.Range(-.5f, .5f); //随机位置

        GameObject CriticalHit = null;
        if (critical)
        {
            CriticalHit = criticalHitFx;
        }
        GameObject newHitFx1 = Instantiate(hitFx, target.position + new Vector3(xPosition,yPosition), Quaternion.identity);

        newHitFx1.transform.Rotate(new Vector3(0,0,zRotation));
        if (CriticalHit != null)
        {
            Debug.Log(critical);
            GameObject newHitFx2 = Instantiate(CriticalHit, target.position + new Vector3(xPosition, yPosition), Quaternion.identity);
            newHitFx2.transform.localScale = new Vector3(GetComponent<Entity>().facedir, 1, 1);
            Destroy(newHitFx2, .5f);

        }

        Destroy(newHitFx1, .5f);
        
    }

    public void CreateFocusFx(Transform target)
    {
        GameObject newFx = Instantiate(focusFx,target.position, Quaternion.identity);
        newFx.transform.SetParent(target);
        Destroy(newFx,3f);
    }

    public void CreateSmokeFx(Transform target)
    {
        GameObject newFx = Instantiate(smokeFx, target.position + new Vector3(3 * GetComponent<Entity>().facedir,0), Quaternion.identity);
        newFx.transform.localScale = new Vector3(GetComponent<Entity>().facedir, 1, 1);

        Destroy(newFx, .2f);
    }
    private IEnumerator CreateAttack1Fx(Transform target)
    {
        yield return new WaitForSeconds(.2f);
        GameObject newFx = Instantiate(attack1Fx, target.position + new Vector3(4 * GetComponent<Entity>().facedir,0), Quaternion.identity);
        newFx.transform.localScale = new Vector3(GetComponent<Entity>().facedir, 1, 1);

        Destroy(newFx, .8f);
    }
    private IEnumerator CreateAttack2Fx(Transform target)
    {
        yield return new WaitForSeconds(0f);
        var entity = GetComponent<Entity>();
        float offsetX = 0f; // 横向偏移，根据需要调整
        Vector3 spawnPos = new Vector3(target.position.x + offsetX * entity.facedir, target.position.y + .5f, target.position.z);
        GameObject newFx = Instantiate(attack2Fx, spawnPos, Quaternion.identity);
        newFx.transform.localScale = new Vector3(entity.facedir, 1, 1);

        Destroy(newFx, .2f);
    }
    private IEnumerator CreateAttack3Fx(Transform target)
    {
        yield return new WaitForSeconds(.2f);
        var entity = GetComponent<Entity>();
        float offsetX = 0.5f; // 横向偏移，根据需要调整
        Vector3 spawnPos = new Vector3(target.position.x + offsetX * entity.facedir, target.position.y + .5f, target.position.z);
        GameObject newFx = Instantiate(attack3Fx, spawnPos, Quaternion.identity);
        newFx.transform.localScale = new Vector3(entity.facedir, 1, 1);

        Destroy(newFx, .8f);
    }
    
    public void CreateAttackFx(Transform target, int attackType)
    {
        CancelAttackFx();
        attackFxRoutine = StartCoroutine(PlayAttackFx(target, attackType));
    }

    public void CancelAttackFx()
    {
        if (attackFxRoutine != null)
        {
            StopCoroutine(attackFxRoutine);
            attackFxRoutine = null;
        }
    }

    private IEnumerator PlayAttackFx(Transform target, int attackType)
    {
        switch (attackType)
        {
            case 0:
                yield return CreateAttack1Fx(target);
                break;
            case 1:
                yield return CreateAttack2Fx(target);
                break;
            case 2:
                yield return CreateAttack3Fx(target);
                break;
            default:
                Debug.LogWarning("Invalid attack type: " + attackType);
                yield break;
        }

        attackFxRoutine = null;
    }
}
