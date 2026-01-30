using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject transitionRoot;      // TransitionRoot (整個物件)
    public Image transitionBG;             // TransitionBG 的 Image
    public Image transitionGraphic;        // TransitionGraphic 的 Image
    public Animator animator;              // 掛在 TransitionRoot 的 Animator

    [Header("Config")]
    public Color bgColor = Color.black;

    public bool IsTransitioning { get; private set; }

    // 給動畫事件呼叫
    public void OnEnterFinished()
    {
        IsTransitioning = false;
    }

    // 播放「黑影 → 彩色」(黑影淡掉)
    public IEnumerator PlayEnter(Sprite silhouette, System.Action onCovered = null)
    {
        if (transitionRoot == null || transitionGraphic == null || animator == null)
        {
            Debug.LogError("[TransitionManager] Missing refs!");
            yield break;
        }

        IsTransitioning = true;

        transitionRoot.SetActive(true);

        if (transitionBG != null) transitionBG.color = bgColor;

        transitionGraphic.sprite = silhouette;

        SetImageAlpha(transitionGraphic, 1f);
        if (transitionBG != null) SetImageAlpha(transitionBG, 1f);

        // ★ 先等一幀，確保黑影真的已經畫到畫面上
        yield return null;

        // ★ 在黑影蓋住的時候換角色資料/監視器圖
        onCovered?.Invoke();

        // ★ 再等一幀保險（可留可不留）
        yield return null;

        animator.ResetTrigger("Enter");
        animator.SetTrigger("Enter");

        // 等動畫事件解除
        while (IsTransitioning) yield return null;

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
