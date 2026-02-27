using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 手把震動需要
using System.Collections;    // ★ 必須加入這個，否則 IEnumerator 會報錯

public enum FeedbackType
{
    Jumpscare,
    Warning,
    LightShake,
    Flicker,
    BossJumpscare
}

public class FeedbackSystem : MonoBehaviour
{
    public static FeedbackSystem Instance;
    public static int FeedbackLevel = 0;

#if UNITY_EDITOR
    [Header("DEBUG (Editor Only)")]
    public bool debugForceHighInEditor = true;
#endif

    [Header("UI 閃爍")]
    public Image flashOverlay;
    public float flashDuration = 0.3f; // 稍微增長一點，效果更柔和

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip jumpscareClip;
    public AudioClip smallClip;       // 小驚嚇音效
    public AudioClip flickerClip;
    public AudioClip warningClip;   // Warning 音效（可不填）

    [Header("Flicker（輕微驚嚇）")]
    public Color flickerColor = Color.white;   // 你也可以改成偏白偏灰
    public float flickerDuration = 0.12f;      // 比 jumpscare 短

    [Header("Light Shake（監視器視覺微震）")]
    public RectTransform shakeTarget;   // 把 camDisplay 的 RectTransform 拖進來
    public float shakeDuration = 0.18f;
    public float shakeStrength = 6f;    // 像素強度（UI座標）
    public int shakeVibrato = 18;       // 抖動頻率

    [Header("Boss Shake Pulse（晃動間隔）")]
    public bool bossShakePulsed = true;
    public float bossShakeOnTime = 0.35f;
    public float bossShakeOffTime = 0.25f;


    [Header("One-shot Rumble（手把短震）")]
    [Range(0f, 1f)] public float oneShotLow = 0.6f;
    [Range(0f, 1f)] public float oneShotHigh = 1.0f;
    public float oneShotDuration = 0.25f;

    [Header("Boss Loop（持續音效）")]
    public AudioSource loopSource;   // 專門播 loop，避免跟 OneShot 打架

    [Header("Boss Rumble（手把持續震動）")]
    [Range(0f, 1f)] public float bossRumbleLow = 0.6f;
    [Range(0f, 1f)] public float bossRumbleHigh = 1.0f;

    [Header("Boss Rumble Pulse（震動間隔）")]
    public bool bossRumblePulsed = true;
    public float bossRumbleOnTime = 0.35f;   // 震多久
    public float bossRumbleOffTime = 0.25f;  // 停多久


    private Coroutine bossShakeCo;
    private Coroutine bossRumbleCo;
    private bool bossActive;

    Coroutine rumbleCo;
    private Coroutine shakeCo;
    private Vector2 _shakeOrigin;

    private Coroutine flashCo;

    public void StartBossPersistent(AudioClip roarLoop, bool enableRumble, float shakeStrength, int shakeVibrato)
    {
        if (bossActive) return;
        bossActive = true;

        // 1) loop roar（中/高回饋才有）
        if (FeedbackLevel >= 1 && loopSource != null && roarLoop != null)
        {
            loopSource.clip = roarLoop;
            loopSource.loop = true;
            loopSource.Play();
        }

        // 2) 持續 shake（一直抖到 Stop）
        if (shakeTarget != null)
        {
            if (bossShakeCo != null) StopCoroutine(bossShakeCo);
            bossShakeCo = StartCoroutine(BossShakeLoop(shakeStrength, shakeVibrato));
        }

        // 3) 持續 rumble（只有高回饋）
        if (enableRumble && FeedbackLevel >= 2)
        {
            var pad = Gamepad.current;
            if (pad != null)
            {
                if (bossRumbleCo != null) StopCoroutine(bossRumbleCo);
                bossRumbleCo = StartCoroutine(BossRumbleLoop(pad, bossRumbleLow, bossRumbleHigh)); // 你可調強度
            }
        }
    }

    public void StopBossPersistent()
    {
        bossActive = false;

        // stop loop audio
        if (loopSource != null)
        {
            loopSource.Stop();
            loopSource.clip = null;
        }

        // stop shake
        if (bossShakeCo != null)
        {
            StopCoroutine(bossShakeCo);
            bossShakeCo = null;
        }
        if (shakeTarget != null) shakeTarget.anchoredPosition = _shakeOrigin;

        // stop rumble
        if (bossRumbleCo != null)
        {
            StopCoroutine(bossRumbleCo);
            bossRumbleCo = null;
        }
        StopRumble();
    }

    IEnumerator BossShakeLoop(float strength, int vibrato)
    {
        if (shakeTarget == null) yield break;

        _shakeOrigin = shakeTarget.anchoredPosition;

        while (bossActive)
        {
            // 如果不想脈衝：維持「一直抖」
            if (!bossShakePulsed)
            {
                float step = 1f / Mathf.Max(1, vibrato);
                float x = Random.Range(-strength, strength);
                float y = Random.Range(-strength, strength);
                shakeTarget.anchoredPosition = _shakeOrigin + new Vector2(x, y);
                yield return new WaitForSeconds(step);
                continue;
            }

            // ===== On：抖一段時間 =====
            float onElapsed = 0f;
            float stepOn = 1f / Mathf.Max(1, vibrato);

            while (bossActive && onElapsed < bossShakeOnTime)
            {
                float x = Random.Range(-strength, strength);
                float y = Random.Range(-strength, strength);
                shakeTarget.anchoredPosition = _shakeOrigin + new Vector2(x, y);

                yield return new WaitForSeconds(stepOn);
                onElapsed += stepOn;
            }

            // ===== Off：回正 + 休息 =====
            shakeTarget.anchoredPosition = _shakeOrigin;
            yield return new WaitForSeconds(bossShakeOffTime);
        }

        // 收尾
        shakeTarget.anchoredPosition = _shakeOrigin;

        //_shakeOrigin = shakeTarget.anchoredPosition;

        //float step = 1f / Mathf.Max(1, vibrato); // 每秒 vibrato 次

        //while (bossActive)
        //{
        //    float x = Random.Range(-strength, strength);
        //    float y = Random.Range(-strength, strength);
        //    shakeTarget.anchoredPosition = _shakeOrigin + new Vector2(x, y);
        //    yield return new WaitForSeconds(step);
        //}

    }

    IEnumerator BossRumbleLoop(Gamepad pad, float low, float high)
    {
        while (bossActive)
        {
            if (!bossRumblePulsed)
            {
                // 原本的「一直震」
                pad.SetMotorSpeeds(low, high);
                yield return null;
                continue;
            }

            // 震一下
            pad.SetMotorSpeeds(low, high);
            yield return new WaitForSeconds(bossRumbleOnTime);

            // 停一下
            pad.SetMotorSpeeds(0f, 0f);
            yield return new WaitForSeconds(bossRumbleOffTime);

            //pad.SetMotorSpeeds(low, high);
            //yield return null; // 每幀維持
        }
        pad.SetMotorSpeeds(0f, 0f);
    }

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

#if UNITY_EDITOR
        if (debugForceHighInEditor)
            FeedbackLevel = 2;   // 0=Low, 1=Mid, 2=High
#endif

        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1, 1, 1, 0);
        }

        if (shakeTarget != null)
            _shakeOrigin = shakeTarget.anchoredPosition;
    }

    public void Trigger(FeedbackType type)
    {
        switch (type)
        {
            case FeedbackType.Jumpscare:
                PlayFlash(Color.white); // 強制白閃
                PlaySound(jumpscareClip);
                PlayRumble(oneShotLow, oneShotHigh, oneShotDuration);
                break;

            case FeedbackType.Flicker:
                flashDuration = flickerDuration;
                PlayFlash(flickerColor);

                // 依版本播放音效：你 PlaySound 本來就會在 FeedbackLevel < 1 時擋掉
                PlaySound(flickerClip);

                // Flicker 通常不震，但你要也可以
                PlayRumble(oneShotLow, oneShotHigh, oneShotDuration);
                break;

            case FeedbackType.Warning:
                // 你可以把 Warning 當成：紅色閃一下 + （中版以上）警告音
                PlayFlash(new Color(1f, 0.2f, 0.2f));   // 偏紅
                PlaySound(warningClip);
                // 通常 Warning 不震動、不大晃
                break;

            case FeedbackType.LightShake:
                // 只做視覺微晃（音效要不要另加看你需求）
                PlaySound(smallClip); // 可以有個小驚嚇音效
                StartShake(shakeDuration, shakeStrength, shakeVibrato);
                PlayRumble(oneShotLow, oneShotHigh, oneShotDuration);

                break;
        }
    }

    void StartShake(float duration, float strength, int vibrato)
    {
        if (shakeTarget == null) return;

        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(duration, strength, vibrato));
    }

    IEnumerator ShakeRoutine(float duration, float strength, int vibrato)
    {
        // 記住原位（避免連續抖造成漂移）
        _shakeOrigin = shakeTarget.anchoredPosition;

        float elapsed = 0f;
        float step = duration / Mathf.Max(1, vibrato);

        while (elapsed < duration)
        {
            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            shakeTarget.anchoredPosition = _shakeOrigin + new Vector2(x, y);

            yield return new WaitForSeconds(step);
            elapsed += step;
        }

        shakeTarget.anchoredPosition = _shakeOrigin;
        shakeCo = null;
    }

    void PlayFlash(Color color)
    {
        if (flashOverlay == null) return;

        if (flashCo != null) StopCoroutine(flashCo);

        flashCo = StartCoroutine(FlashRoutine(color));
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
        flashCo = null;

    }

    void PlaySound(AudioClip clip)
    {
        if (FeedbackLevel < 1 || clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}