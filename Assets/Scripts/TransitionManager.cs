using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup transitionCanvas;   // 只負責透明度

    [Header("Timing (seconds)")]
    public float fadeInDuration = 0.5f;
    public float holdDuration = 0.6f;
    public float fadeOutDuration = 0.5f;

    [Header("Color")]
    public Image transitionBG;         // 指向 TransitionBG 的 Image
    public Color fadeColor = Color.black; // 你想要的顏色
    public bool IsTransitioning { get; private set; } = false;

    public IEnumerator PlayTransition()
    {
        IsTransitioning = true;
        transitionCanvas.gameObject.SetActive(true);

        // Color
        if (transitionBG != null)
            transitionBG.color = fadeColor; 

        // Fade In
        yield return FadeAlpha(0.5f, 1f, fadeInDuration);

        // Hold
        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        // Fade Out
        yield return FadeAlpha(1f, 0.5f, fadeOutDuration);



        transitionCanvas.alpha = 0f;
        transitionCanvas.gameObject.SetActive(false);
        IsTransitioning = false;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            transitionCanvas.alpha = to;
            yield break;
        }

        float t = 0f;
        transitionCanvas.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        transitionCanvas.alpha = to;
    }
}
