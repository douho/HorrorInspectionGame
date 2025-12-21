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
}
