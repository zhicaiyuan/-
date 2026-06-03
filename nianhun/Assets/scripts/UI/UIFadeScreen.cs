using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeScreen : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (anim == null)
            Debug.LogWarning("UIFadeScreen: Animator 未找到，请在 Inspector 中赋值或将 Animator 添加到同一 GameObject。", this);
    }

    public void FadeOut()
    {
        if (anim == null)
            return;
        anim.SetTrigger("FadeOut");
    }

    public void FadeIn()
    {
        if (anim == null)
            return;
        anim.SetTrigger("FadeIn");
    }
}
