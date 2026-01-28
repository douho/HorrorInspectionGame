using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareSequenceRunner : MonoBehaviour
{
    [Header("Refs")]
    public GameFlowController gameFlow;     // 指向自己場景中的 GameFlowController
    public CamSwitchController cam;         // camController
    public TransitionManager transition;    // transitionManager

    // 防止同一瞬間連續觸發
    [Header("Debounce")]
    public float camTriggerDebounce = 0.05f;
    private float _lastCamTriggerTime = -999f;

    // 一次只跑一條
    private Coroutine _running;
    private readonly Queue<FeedbackStep> _queue = new Queue<FeedbackStep>();

    void OnEnable()
    {
        CamSwitchController.OnCamChanged += OnCamChanged;
    }

    void OnDisable()
    {
        CamSwitchController.OnCamChanged -= OnCamChanged;
    }

    void OnCamChanged(int camIndex)
    {
        if (gameFlow == null) return;
        if (gameFlow.IsSpawning) return;

        // 1) 基本防呆
        var ch = gameFlow.currentCharacter;
        if (ch == null) return;
        var seq = ch.jumpScareSequence;
        if (seq == null) return;

        // 2) 過場期間不觸發（避免黑屏把 scare 吃掉）
        if (transition != null && transition.IsTransitioning) return;

        // 3) 全域鎖期間不觸發（你如果想改成「延後觸發」也可以，先用保守策略）
        if (InteractionLock.GlobalLock) return;

        // 4) debounce：避免 L1/R1 很快切造成同幀多次
        if (Time.time - _lastCamTriggerTime < camTriggerDebounce) return;
        _lastCamTriggerTime = Time.time;

        // 5) 讓 sequence 回傳這次 cam 對應的 steps（會自帶「已觸發不再回傳」）
        List<FeedbackStep> steps = seq.ConsumeStepsForCam(camIndex);
        if (steps == null || steps.Count == 0) return;

        // 6) 入 queue
        foreach (var s in steps) _queue.Enqueue(s);

        // 7) 如果沒在跑，就開始跑
        if (_running == null)
            _running = StartCoroutine(RunQueue());
    }

    IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            var step = _queue.Dequeue();
            // 直接用你現有的 ExecuteStep（先不改你的演出邏輯）
            yield return gameFlow.StartCoroutine(gameFlow.ExecuteStep(step));
        }
        _running = null;
    }
}
