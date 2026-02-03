using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject transitionRoot;
    public Image transitionBG;
    public Image transitionGraphic;
    public Animator animator;
    public CanvasGroup canvasGroup;        // ★新增：如果你有 CanvasGroup（你之前有）

    [Header("Config")]
    public Color bgColor = Color.black;

    public bool IsTransitioning { get; private set; }

    // 給動畫事件呼叫
    public void OnEnterFinished()
    {
        IsTransitioning = false;
    }

    public IEnumerator PlayEnter(Sprite silhouette, Sprite colorSprite, System.Action onCovered = null)

    {
        if (transitionRoot == null || transitionGraphic == null || animator == null)
        {
            Debug.LogError("[TransitionManager] Missing refs!");
            yield break;
        }

        IsTransitioning = true;

        transitionRoot.SetActive(true);

        // ★確保 CanvasGroup 不是 0（不然你會什麼都看不到）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }

        if (transitionBG != null)
        {
            transitionBG.sprite = colorSprite;    // ★彩圖底
            transitionBG.color = Color.white;     // ★不要染色，保持原圖
            SetImageAlpha(transitionBG, 1f);      // ★底圖要全亮
        }

        transitionGraphic.sprite = silhouette;
        transitionGraphic.color = Color.white;
        SetImageAlpha(transitionGraphic, 1f);

        // ★先等一幀，確保黑影真的畫到畫面上
        yield return null;

        // ★在黑影蓋住時換資料（避免穿幫）
        yield return new WaitForSeconds(0.2f);
        onCovered?.Invoke();

        yield return null;

        animator.ResetTrigger("Enter");
        animator.SetTrigger("Enter");

        // ★安全機制：避免動畫事件沒打到就永遠卡死
        float safety = 5f;  
        while (IsTransitioning && safety > 0f)
        {
            safety -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (IsTransitioning)
        {
            Debug.LogWarning("[TransitionManager] Enter animation event not fired, force continue.");
            IsTransitioning = false;
        }

        // 收尾
        //if (canvasGroup != null) canvasGroup.alpha = 0f;
        transitionRoot.SetActive(false);
    }

    void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
