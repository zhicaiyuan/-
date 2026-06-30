using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class UIFadeScreen : MonoBehaviour
{
    [SerializeField] private Image overlay;
    [SerializeField] private Animator anim;
    [SerializeField] private float defaultFadeDuration = 1.2f;

    private Coroutine fadeRoutine;
    private bool fadeCancelled;

    public bool IsFadeCancelled => fadeCancelled;

    private void Awake()
    {
        if (overlay == null)
            overlay = GetComponent<Image>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (SceneTransitionData.ShouldHoldBlackOnLoad())
            SetAlpha(1f);
        else
            SetAlpha(0f);
    }

    public void SetAlpha(float alpha)
    {
        if (fadeCancelled || overlay == null)
            return;

        ApplyAlpha(alpha);
    }

    public void SetBlackInstant()
    {
        if (fadeCancelled)
            return;

        ApplyAlpha(1f);
    }

    public void SetClearInstant() => ApplyAlpha(0f);

    public void CancelFadeAndClear()
    {
        fadeCancelled = true;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        DisableAnimator();
        ApplyAlpha(0f);
    }

    public void ResumeFading()
    {
        fadeCancelled = false;
    }

    private void ApplyAlpha(float alpha)
    {
        if (overlay == null)
            return;

        Color color = overlay.color;
        color.a = Mathf.Clamp01(alpha);
        overlay.color = color;
    }

    public void FadeOut()
    {
        StartFade(FadeOutRoutine(defaultFadeDuration));
    }

    public void FadeIn()
    {
        StartFade(FadeInRoutine(defaultFadeDuration));
    }

    public void FadeOut(float duration)
    {
        StartFade(FadeOutRoutine(duration));
    }

    public void FadeIn(float duration)
    {
        StartFade(FadeInRoutine(duration));
    }

    public IEnumerator FadeOutRoutine(float duration)
    {
        DisableAnimator();

        float elapsed = 0f;
        float startAlpha = overlay != null ? overlay.color.a : 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            if (fadeCancelled)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, 1f, elapsed / duration));
            yield return null;
        }

        if (!fadeCancelled)
            SetBlackInstant();
    }

    public IEnumerator FadeInRoutine(float duration, float delay = 0f)
    {
        DisableAnimator();

        if (delay > 0f)
        {
            float waited = 0f;
            while (waited < delay)
            {
                if (fadeCancelled)
                    yield break;

                waited += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (fadeCancelled)
            yield break;

        SetBlackInstant();

        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            if (fadeCancelled)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }

        if (!fadeCancelled)
            SetClearInstant();
    }

    private void StartFade(IEnumerator routine)
    {
        fadeCancelled = false;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(WrapRoutine(routine));
    }

    private IEnumerator WrapRoutine(IEnumerator routine)
    {
        yield return routine;
        fadeRoutine = null;
    }

    private void DisableAnimator()
    {
        if (anim != null)
            anim.enabled = false;
    }
}
