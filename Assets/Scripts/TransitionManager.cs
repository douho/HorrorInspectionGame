using UnityEngine;
using System.Collections;


public class TransitionManager : MonoBehaviour
{
    public CanvasGroup transitionCanvas;   // 過場黑幕或過場圖片
    public bool IsTransitioning { get; private set; } = false; 
    public IEnumerator PlayTransition()
    {
        IsTransitioning = true;

        transitionCanvas.gameObject.SetActive(true);

        // ★ Fade In（畫面變黑或顯示過場圖）
        for (float t = 0; t < 1f; t += Time.deltaTime * 1.5f)
        {
            transitionCanvas.alpha = t;
            yield return null;
        }
        transitionCanvas.alpha = 1f;

        // ★ 停留 0.6 秒（可自行調整）
        yield return new WaitForSeconds(0.6f);

        // ★ Fade Out（畫面淡出，露出下一位訪客）
        for (float t = 1f; t > 0f; t -= Time.deltaTime * 1.5f)
        {
            transitionCanvas.alpha = t;
            yield return null;
        }
        transitionCanvas.alpha = 0f;

        transitionCanvas.gameObject.SetActive(false);

        IsTransitioning = false;
    }
}
