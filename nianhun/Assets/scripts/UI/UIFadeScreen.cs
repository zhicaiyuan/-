using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeScreen : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void FadeOut() => anim.SetTrigger("FadeOut");
    public void FadeIn() => anim.SetTrigger("FadeIn");
}
