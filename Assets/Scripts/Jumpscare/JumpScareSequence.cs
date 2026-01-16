using System.Collections.Generic;
using UnityEngine;

public enum TriggerCondition { OnCamSwitch, OnDecision }

[CreateAssetMenu(menuName = "Game/JumpScareSequence")]
public class JumpScareSequence : ScriptableObject
{
    public TriggerCondition triggerCondition;
    public FeedbackStep[] steps;

    // 用來記錄哪些步驟已經執行過，避免重複觸發
    private bool[] triggeredSteps;

    public void ResetSequence()
    {
        if (steps != null)
            triggeredSteps = new bool[steps.Length];
    }

    //public void TriggerIfMatchCam(int camIndex)
    //{
    //    if (steps == null || steps.Length == 0) return;

    //    // 安全檢查，確保陣列已初始化
    //    if (triggeredSteps == null || triggeredSteps.Length != steps.Length)
    //        triggeredSteps = new bool[steps.Length];

    //    for (int i = 0; i < steps.Length; i++)
    //    {
    //        // 如果該步驟已經跑過了，直接跳過
    //        if (triggeredSteps[i]) continue;

    //        var step = steps[i];

    //        // 檢查條件：必須是相機觸發模式，且相機編號符合
    //        if (triggerCondition == TriggerCondition.OnCamSwitch && step.triggerCamIndex == camIndex)
    //        {
    //            triggeredSteps[i] = true; // 標記為已觸發
    //            Debug.Log($"[JumpScare] 觸發第 {i} 步，目標相機: {camIndex}");

    //            // 透過 GameFlowController 執行該步驟（含 delay）
    //            GameFlowController.Instance.StartCoroutine(GameFlowController.Instance.ExecuteStep(step));
    //        }
    //    }
    //}
    public List<FeedbackStep> ConsumeStepsForCam(int camIndex)
    {
        if (steps == null || steps.Length == 0) return null;

        if (triggeredSteps == null || triggeredSteps.Length != steps.Length)
            triggeredSteps = new bool[steps.Length];

        List<FeedbackStep> result = null;

        for (int i = 0; i < steps.Length; i++)
        {
            if (triggeredSteps[i]) continue;

            var step = steps[i];

            if (triggerCondition == TriggerCondition.OnCamSwitch && step.triggerCamIndex == camIndex)
            {
                triggeredSteps[i] = true;

                result ??= new List<FeedbackStep>();
                result.Add(step);
            }
        }

        return result;
    }

}

[System.Serializable]
public struct FeedbackStep
{
    public FeedbackType feedbackType;
    public float delay;
    public int triggerCamIndex;      // 指定觸發的相機 (0, 1, 2...)
    public bool silenceAudio;
    public Sprite overrideImage;     // 監視器畫面覆蓋
    public Sprite jumpscareImage;    // 大圖 Jumpscare
}