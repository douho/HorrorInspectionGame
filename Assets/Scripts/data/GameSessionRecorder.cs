using System;
using System.Collections.Generic;
using UnityEngine;

public enum FeedbackCondition { Low, Mid, High }
public enum PlayerDecision { Allow, Reject }

[Serializable]
public class DecisionRecord
{
    public int characterId;
    public bool groundTruthShouldAllow;
    public PlayerDecision playerDecision;
    public bool isCorrect;

    public float timeFromRoundStartSec;

    public bool[] checklistAnswers;      // 長度 3
    public bool[] checklistCorrectness;  // 長度 3
}

public class GameSessionRecorder : MonoBehaviour
{
    public static GameSessionRecorder Instance { get; private set; }

    [Header("Session")]
    public string sessionId;
    public string participantId;
    public string buildVersion;
    public FeedbackCondition feedbackCondition = FeedbackCondition.Low;

    [Header("Runtime")]
    public List<DecisionRecord> records = new List<DecisionRecord>();

    private DecisionRecord current;
    private float roundStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionId = Guid.NewGuid().ToString();
        buildVersion = Application.version;
    }

    public void StartRound(CharacterDefinition ch)
    {
        current = new DecisionRecord
        {
            characterId = ch.characterId,
            groundTruthShouldAllow = ch.shouldAllow,
            checklistAnswers = new bool[3],
            checklistCorrectness = new bool[3],
        };
        roundStartTime = Time.time;
    }

    public void SetChecklist(bool[] answers, bool[] correctness)
    {
        if (current == null) return;

        current.timeFromRoundStartSec = Time.time - roundStartTime;

        Array.Copy(answers, current.checklistAnswers, 3);
        Array.Copy(correctness, current.checklistCorrectness, 3);
    }

    public void FinalizeDecision(bool approve)
    {
        if (current == null) return;

        current.playerDecision = approve ? PlayerDecision.Allow : PlayerDecision.Reject;
        current.isCorrect = (approve == current.groundTruthShouldAllow);

        records.Add(current);
        current = null;
    }

    public int CorrectCount()
    {
        int c = 0;
        foreach (var r in records) if (r.isCorrect) c++;
        return c;
    }

    public int WrongCount() => records.Count - CorrectCount();
}
