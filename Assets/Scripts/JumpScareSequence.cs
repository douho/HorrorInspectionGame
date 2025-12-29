// Assets/Scripts/Data/JumpScareSequence.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TriggerCondition { OnCamSwitch, OnDecision }

[CreateAssetMenu(menuName = "Game/JumpScareSequence")]
public class JumpScareSequence : ScriptableObject
{
    public TriggerCondition triggerCondition;
    public FeedbackStep[] steps;

    private bool[] triggeredSteps;
    private HashSet<int> triggeredCamSteps = new HashSet<int>();



    private void Awake()
    {
        triggeredSteps = new bool[steps.Length];
    }

    public void TriggerIfMatchCam(int camIndex)
    {
        Debug.Log($"[JumpScare] 檢查觸發條件：camIndex = {camIndex}, step 數 = {steps.Length}");

        // 防止 ScriptableObject 的 Awake 沒有執行，導致 triggeredSteps 是 null
        if (triggeredSteps == null || triggeredSteps.Length != steps.Length)
            triggeredSteps = new bool[steps.Length];

        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];
            Debug.Log($"[JumpScare] 檢查 step {i}：triggerCamIndex = {step.triggerCamIndex}, 已觸發 = {triggeredSteps[i]}");

            if (triggeredSteps[i]) continue;

            if (triggerCondition == TriggerCondition.OnCamSwitch && step.triggerCamIndex == camIndex)
            {
                triggeredSteps[i] = true;
                Debug.Log($"[JumpScare] 觸發第 {i} 個步驟，cam = {camIndex}, delay = {step.delay}");
                GameFlowController.Instance.StartCoroutine(GameFlowController.Instance.ExecuteStep(step));
            }
        }
    }





    public void Init()
    {
        triggeredCamSteps.Clear();
    }
    public void ResetSequence()
    {
        triggeredCamSteps.Clear();
        if (steps != null)
        triggeredSteps = new bool[steps.Length]; // 全部重置為 false
    }



}

[System.Serializable]
public struct FeedbackStep
{
    public FeedbackType feedbackType;
    public float delay;

    // 可選欄位
    public int triggerCamIndex;      // -1 代表不指定（若指定，代表切到該 CAM 才執行）
    public bool silenceAudio;        // true = 停止音效與音樂
    public Sprite overrideImage;     // 若有，強制覆蓋該 CAM 畫面（例如黑圖）

    public Sprite jumpscareImage; // 給 FeedbackType.Jumpscare 用（大驚嚇）

}



