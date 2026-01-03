using UnityEngine;
using UnityEngine.UI;

public class JumpscareController : MonoBehaviour
{
    public GameObject jumpscareImage;
    public float showTime = 1.0f;
    public Sprite defaultJumpscareImage;

    private void Awake()
    {
        if (jumpscareImage != null) jumpscareImage.SetActive(false);
    }

    public void TriggerJumpscare(Sprite img = null)
    {
        if (jumpscareImage == null) return;

        var uiImage = jumpscareImage.GetComponent<Image>();
        if (uiImage != null)
        {
            uiImage.sprite = img != null ? img : defaultJumpscareImage;
            // 確保透明度為 1
            Color c = uiImage.color;
            c.a = 1f;
            uiImage.color = c;
        }

        jumpscareImage.SetActive(true);

        // ★ 這裡移除了原始程式碼中的 FeedbackSystem 呼叫，避免重複觸發衝突

        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), showTime);
    }

    private void Hide()
    {
        if (jumpscareImage != null) jumpscareImage.SetActive(false);
    }
}