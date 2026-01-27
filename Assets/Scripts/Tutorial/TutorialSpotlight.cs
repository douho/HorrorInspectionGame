using UnityEngine;
using UnityEngine.UI;

public class TutorialSpotlight : MonoBehaviour
{
    [Header("Refs")]
    public RawImage overlayImage;                 // 全螢幕 Image（掛 Spotlight 材質）
    public RectTransform target;               // 要聚焦的 UI（手冊 icon）
    public Canvas rootCanvas;                  // 你的主 Canvas（用來抓正確 camera）

    [Header("Hole")]
    [Range(0.01f, 0.5f)] public float holeRadius = 0.10f;
    [Range(0.0f, 0.2f)] public float softness = 0.02f;

    private Material runtimeMat;

    void Awake()
    {
        if (overlayImage == null) overlayImage = GetComponentInChildren<RawImage>(true);
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();

        if (overlayImage != null)
        {
            // runtime 拷貝，避免改到共用材質
            if (overlayImage.material != null)
            {
                runtimeMat = Instantiate(overlayImage.material);
                overlayImage.material = runtimeMat;
            }

            // 不要擋住 UI 點擊（這是 spotlight 常見需求）
            overlayImage.raycastTarget = false;
        }

        Hide();
    }

    void LateUpdate()
    {
        if (overlayImage == null) { Debug.LogError("[Spotlight] overlayImage null"); return; }
        if (runtimeMat == null) { Debug.LogError("[Spotlight] runtimeMat null (material missing?)"); return; }
        if (target == null) { Debug.LogWarning("[Spotlight] target null"); return; }

        // 用 overlayImage 的 active 狀態判斷（不是 gameObject.activeSelf）
        if (overlayImage == null || !overlayImage.gameObject.activeInHierarchy) return;
        if (target == null || runtimeMat == null) return;

        // target 的中心點 -> 螢幕座標 -> 0~1 UV
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f;

        Camera cam = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, centerWorld);
        Vector2 uv = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);

        runtimeMat.SetVector("_HoleCenter", new Vector4(uv.x, uv.y, 0, 0));
        runtimeMat.SetFloat("_HoleRadius", holeRadius);
        runtimeMat.SetFloat("_Softness", softness);
    }

    public void Show(RectTransform focusTarget)
    {
        target = focusTarget;
        if (overlayImage != null) overlayImage.gameObject.SetActive(true);
        Debug.Log($"[Spotlight] Show target={target?.name}, overlayActive={overlayImage.gameObject.activeInHierarchy}, mat={runtimeMat != null}");

    }

    public void Hide()
    {
        if (overlayImage != null) overlayImage.gameObject.SetActive(false);
        target = null;
    }
}
