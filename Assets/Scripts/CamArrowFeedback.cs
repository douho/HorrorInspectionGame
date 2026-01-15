using UnityEngine;
using UnityEngine.InputSystem;

public class CamArrowFeedback : MonoBehaviour
{
    [Header("Targets")]
    public RectTransform leftArrow;   // btnPrev 的 RectTransform
    public RectTransform rightArrow;  // btnNext 的 RectTransform
    public CanvasGroup leftCanvasGroup;   // 可選：用來做亮暗
    public CanvasGroup rightCanvasGroup;  // 可選：用來做亮暗

    [Header("Scale")]
    public float pressedScale = 0.9f;     // 按下縮到 90%
    public float lerpSpeed = 18f;         // 彈性速度

    [Header("Alpha (optional)")]
    public float pressedAlpha = 0.65f;    // 按下變暗/變亮用 alpha
    float _leftBaseAlpha = 1f;
    float _rightBaseAlpha = 1f;

    Vector3 _leftBaseScale = Vector3.one;
    Vector3 _rightBaseScale = Vector3.one;

    void Awake()
    {
        if (leftArrow != null) _leftBaseScale = leftArrow.localScale;
        if (rightArrow != null) _rightBaseScale = rightArrow.localScale;

        if (leftCanvasGroup != null) _leftBaseAlpha = leftCanvasGroup.alpha;
        if (rightCanvasGroup != null) _rightBaseAlpha = rightCanvasGroup.alpha;
    }

    void Update()
    {
        var pad = Gamepad.current;
        if (pad == null) return;

        bool lHeld = pad.leftShoulder.isPressed;
        bool rHeld = pad.rightShoulder.isPressed;

        Apply(leftArrow, leftCanvasGroup, _leftBaseScale, _leftBaseAlpha, lHeld);
        Apply(rightArrow, rightCanvasGroup, _rightBaseScale, _rightBaseAlpha, rHeld);
    }

    void Apply(RectTransform rt, CanvasGroup cg, Vector3 baseScale, float baseAlpha, bool held)
    {
        if (rt != null)
        {
            var target = held ? baseScale * pressedScale : baseScale;
            rt.localScale = Vector3.Lerp(rt.localScale, target, Time.deltaTime * lerpSpeed);
        }

        if (cg != null)
        {
            float targetA = held ? pressedAlpha : baseAlpha;
            cg.alpha = Mathf.Lerp(cg.alpha, targetA, Time.deltaTime * lerpSpeed);
        }
    }
}
