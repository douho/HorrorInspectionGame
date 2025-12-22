using UnityEngine;
using UnityEngine.InputSystem; // 手把震動需要
using UnityEngine.UI;

public enum FeedbackType
{
    Jumpscare,
    Warning,
    LightShake
}

public class FeedbackSystem : MonoBehaviour
{
    public static FeedbackSystem Instance;

    [Header("回饋強度 0=低, 1=中, 2=高（MainMenu設定）")]
    public static int FeedbackLevel = 0;

    [Header("UI")]
    public Image flashOverlay; // 白色或紅色閃光用
    public float flashDuration = 0.2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpscareClip;
    public AudioClip warningClip;

    private void Awake()
    {
        Debug.Log(Gamepad.current);
        Instance = this;
        if (flashOverlay != null)
            flashOverlay.color = new Color(1, 1, 1, 0); // 初始透明
    }

    // 統一觸發回饋方法
    public void Trigger(FeedbackType type)
    {
        switch (type)
        {
            case FeedbackType.Jumpscare:
                PlayFlash(Color.white);
                PlaySound(jumpscareClip);
                DoVibration(0.4f, 0.5f); // 強震動（高回饋限定）
                break;

            case FeedbackType.Warning:
                PlayFlash(new Color(1, 0, 0, 0.4f));
                PlaySound(warningClip);
                DoVibration(0.2f, 0.3f);
                break;

            case FeedbackType.LightShake:
                PlayFlash(new Color(1, 1, 1, 0.15f));
                DoVibration(0.1f, 0.2f);
                break;
        }
    }

    // ====== 畫面變化 ======
    void PlayFlash(Color color)
    {
        if (flashOverlay == null) return;

        StopAllCoroutines();
        StartCoroutine(FlashRoutine(color));
    }

    System.Collections.IEnumerator FlashRoutine(Color color)
    {
        flashOverlay.color = color;
        float t = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0, t / flashDuration);
            flashOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        flashOverlay.color = new Color(color.r, color.g, color.b, 0);
    }

    // ====== 音效 ======
    void PlaySound(AudioClip clip)
    {
        if (FeedbackLevel < 1) return; // 低回饋不播放音效
        if (clip == null || audioSource == null) return;

        audioSource.PlayOneShot(clip);
    }
    public void Mute()
    {
        if (audioSource != null)
            audioSource.Stop();
    }


    // ====== 手把震動 ======
    void DoVibration(float lowFreq, float highFreq)
    {
        if (FeedbackLevel < 2) return; // 只有高回饋才震動

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
            Invoke(nameof(StopVibration), 0.4f);
        }
    }

    void StopVibration()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0, 0);
    }
}
