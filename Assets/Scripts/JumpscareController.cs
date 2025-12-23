using UnityEngine;

public class JumpscareController : MonoBehaviour
{
    public GameObject jumpscareImage;  // 一張大圖，平常關閉
    public float showTime = 1.0f;

    public Sprite defaultJumpscareImage; // 預設的 jumpscare 圖片，按J跳出


    private void Awake()
    {
        jumpscareImage.SetActive(false);
    }

    public void TriggerJumpscare(Sprite img)
    {
        if (img != null)
        {
            var image = jumpscareImage.GetComponent<UnityEngine.UI.Image>();
            image.sprite = img;
            //image.color = new Color(1, 1, 1, 1); // ← 加這行，確保圖片不透明
        }

        // 顯示大圖
        jumpscareImage.SetActive(true);

        // 呼叫 FeedbackSystem（依照低/中/高自動給不同回饋）
        FeedbackSystem.Instance.Trigger(FeedbackType.Jumpscare);

        // 幾秒後關閉 jumpscare 圖片
        Invoke(nameof(Hide), showTime);

    }

    private void Hide()
    {
        jumpscareImage.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) // 按 J 播 jumpscare
        {
            TriggerJumpscare(defaultJumpscareImage);
        }
    }

}
