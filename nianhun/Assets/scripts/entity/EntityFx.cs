using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityFx : MonoBehaviour
{
    private SpriteRenderer sr;
    [Header("flashfx")]
    [SerializeField] private Material Hitmat;
     private Material originalmat;

    [Header("Ailment colors")]
    [SerializeField] private Color[] chillcolor;
    [SerializeField] private Color[] firecolor;
    [SerializeField] private Color[] shockColor;
    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalmat = sr.material;

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
}
