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
    private HashSet<int> triggeredCamSteps = new();


    private void Awake()
    {
        triggeredSteps = new bool[steps.Length];
    }

    public void TriggerIfMatchCam(int camIndex)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];
            if (step.triggerCamIndex != camIndex) continue;
            if (triggeredCamSteps.Contains(i)) continue;

            triggeredCamSteps.Add(i); // 標記此步驟已觸發
            GameFlowController.Instance.StartCoroutine(GameFlowController.Instance.ExecuteStep(step));
        }
    }

    public void Init()
    {
        triggeredCamSteps.Clear();
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



