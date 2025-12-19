using UnityEngine;

public class JumpscareController : MonoBehaviour
{
    public GameObject jumpscareImage;  // 一張大圖，平常關閉
    public float showTime = 0.4f;

    private void Awake()
    {
        jumpscareImage.SetActive(false);
    }

    public void TriggerJumpscare()
    {
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
            TriggerJumpscare();
        }
    }

}
