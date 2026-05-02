using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UIVolumeSlider : MonoBehaviour
{
    public Slider slider;
    public string parametr;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float multiplier;


    public void SliderValue(float value) => audioMixer.SetFloat(parametr, Mathf.Log10(value) * multiplier);//设置滑块改变

    public void LoadSlider(float value)
    {
        if(value >= 0.001f)
            slider.value = value;
    }
}
