// Assets/Scripts/Data/JumpScareSequence.cs
using UnityEngine;

public enum TriggerCondition { OnCamSwitch, OnDecision }

[CreateAssetMenu(menuName = "Game/JumpScareSequence")]
public class JumpScareSequence : ScriptableObject
{
    public TriggerCondition triggerCondition;
    public FeedbackStep[] steps;
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

