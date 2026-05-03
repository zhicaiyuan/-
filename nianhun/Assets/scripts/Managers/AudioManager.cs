using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private float diastanceToSound;
    [SerializeField] private AudioSource[] sfx;
    [SerializeField] private AudioSource[] bgm;

    public bool playBgm;
    public int bgmIndex;
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }
    private void Update()
    {
        if(!playBgm)
            StopAllBGM();
        else
        {
            if (!bgm[bgmIndex].isPlaying)
                PlayBGM(bgmIndex);
        }
    }

    public void PlaySFX(int sfxindex,Transform source)
    {
        if (sfx[sfxindex].isPlaying)
            sfx[sfxindex].Stop();

        if (source != null && Vector2.Distance(playermanger.instance.player.transform.position, source.position) > diastanceToSound)
            return;

        if(sfxindex < sfx.Length)
        {
            sfx[sfxindex].pitch = Random.Range(0.85f, 1.1f);
            sfx[sfxindex].Play();
        }
    }//播放音频

    public void StopSFX(int sfxindex) => sfx[sfxindex].Stop();

    public void StopSFXWithTime(int index) => StartCoroutine(DecreaseVolume(sfx[index]));//慢慢减小并停止
    private IEnumerator DecreaseVolume(AudioSource audio)
    {
        float defaultVolume = audio.volume;
        while(audio.volume > .1f)
        {
            audio.volume -= audio.volume * .2f;
            yield return new WaitForSeconds(.25f);

            if(audio.volume < .1f)
            { 
                audio.Stop();
                audio.volume = defaultVolume;
                break;
            }
        } 
    }
    public void PlayRandomBGM()
    {
        bgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(bgmIndex);
    }//随机播放
    public void PlayBGM(int bgmindex)
    {
        bgmIndex = bgmindex;

        StopAllBGM();
        bgm[bgmIndex].Play();
    }//播放背景音

    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }//结束背景音
}
