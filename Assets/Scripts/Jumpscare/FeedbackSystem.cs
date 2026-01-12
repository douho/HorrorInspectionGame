using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 手把震動需要
using System.Collections;    // ★ 必須加入這個，否則 IEnumerator 會報錯

public enum FeedbackType
{
    Jumpscare,
    Warning,
    LightShake
}

public class FeedbackSystem : MonoBehaviour
{
    public static FeedbackSystem Instance;
    public static int FeedbackLevel = 0;

    [Header("UI 閃爍")]
    public Image flashOverlay;
    public float flashDuration = 0.3f; // 稍微增長一點，效果更柔和

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip jumpscareClip;

    Coroutine rumbleCo;

    void PlayRumble(float lowFreq, float highFreq, float duration)
    {
        // 只在 High 版震動：FeedbackLevel == 2
        if (FeedbackLevel < 2) return;

        var pad = Gamepad.current;
        if (pad == null) return;

        // 停掉前一次震動，避免疊加失控
        if (rumbleCo != null) StopCoroutine(rumbleCo);
        rumbleCo = StartCoroutine(RumbleRoutine(pad, lowFreq, highFreq, duration));
    }

    IEnumerator RumbleRoutine(Gamepad pad, float low, float high, float duration)
    {
        pad.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0f, 0f);
        rumbleCo = null;
    }

    void StopRumble()
    {
        var pad = Gamepad.current;
        if (pad != null) pad.SetMotorSpeeds(0f, 0f);

        if (rumbleCo != null)
        {
            StopCoroutine(rumbleCo);
            rumbleCo = null;
        }
    }

    // 建議加在 OnDisable / OnDestroy，避免切場景後還在震
    private void OnDisable() => StopRumble();
    private void OnDestroy() => StopRumble();


    private void Awake()
    {
        Instance = this;
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1, 1, 1, 0);
        }
    }

    public void Trigger(FeedbackType type)
    {
        switch (type)
        {
            case FeedbackType.Jumpscare:
                PlayFlash(Color.white); // 強制白閃
                PlaySound(jumpscareClip);
                break;
        }
    }

    void PlayFlash(Color color)
    {
        if (flashOverlay == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(color));
    }

    IEnumerator FlashRoutine(Color targetColor)
    {
        // ★ 強制將 Alpha 設為 1 (完全不透明) 作為閃爍開頭
        flashOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, newAlpha);
            yield return null;
        }
        flashOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
    }

    void PlaySound(AudioClip clip)
    {
        if (FeedbackLevel < 1 || clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}