using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Data")]
    public CharacterDatabase characterDB;
    public CharacterDefinition currentCharacter;

    [Header("UI")]
    public IDCardUI idCardUI;
    public CheckListUI checkListUI;
    public GameObject decisionUI;
    public CamSwitchController camController;
    public TransitionManager transitionManager;
    public JumpscareController jumpscareController;

    private int currentIndex = -1;
    public static GameFlowController Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (decisionUI != null) decisionUI.SetActive(false);
        if (checkListUI != null) checkListUI.Deactivate();
        StartNext();
    }

    private Coroutine startNextCo;

    private IEnumerator StartNextRoutine()
    {
        // 避免重複呼叫
        if (startNextCo != null) yield break;
        startNextCo = StartCoroutine(_StartNextRoutine());
        yield return startNextCo;
        startNextCo = null;
    }

    private IEnumerator _StartNextRoutine()
    {
        if (camController != null)
        {
            camController.ClearRuntimeOverrides();
            camController.ForceSwitchTo(0, invokeEvent: false);

            // ★ 關鍵：立刻指定一張「空畫面 / 黑畫面」
            camController.camDisplay.sprite = null;
        }
        // 1) 鎖住一切操作
        InteractionLock.GlobalLock = true;
        InteractionLock.CameraLock = true;
        FocusManager.FocusLock = true;

        // 2) 播過場（如果你有接 TransitionManager）
        if (transitionManager != null)
        {
            yield return StartCoroutine(transitionManager.PlayTransition());
        }
        else
        {
            // 沒有 transition 也給一幀，避免輸入同幀穿透
            yield return null;
        }

        // 3) 清理 UI（你原本的）
        if (TutorialManager.TutorialFinished)
        {
            var dm = FindObjectOfType<DialogueManager>();
            if (dm != null) dm.HideDialogue();
        }

        if (checkListUI != null)
        {
            checkListUI.ClearCheckList();
            checkListUI.Deactivate();
        }
        if (decisionUI != null) decisionUI.SetActive(false);

        // 4) 換角色
        currentIndex++;
        var ch = characterDB.GetByIndex(currentIndex);
        currentCharacter = ch;

        if (ch == null)
        {
            // 結束場景前解鎖避免卡住（雖然會換場景）
            InteractionLock.GlobalLock = false;
            InteractionLock.CameraLock = false;
            FocusManager.FocusLock = false;

            ShowEnding();
            yield break;
        }

        if (GameSessionRecorder.Instance != null)
            GameSessionRecorder.Instance.StartRound(ch);

        if (ch.jumpScareSequence != null)
            ch.jumpScareSequence.ResetSequence();

        if (idCardUI != null)
            idCardUI.SetCard(ch.idCard);

        if (camController != null)
        {
            camController.SetCamImages(ch.monitorImages, ch);

            // ★關鍵：過場結束後才觸發 CAM001，讓 D 的 delay 從這裡開始算
            camController.ForceSwitchTo(0, invokeEvent: true);
        }

        // 5) 解鎖
        InteractionLock.GlobalLock = false;
        InteractionLock.CameraLock = false;
        FocusManager.FocusLock = false;
    }

    public void StartNext()
    {
        StartCoroutine(StartNextRoutine());
    }

    public IEnumerator ExecuteStep(FeedbackStep step)
    {
        // 1. 執行基礎延遲（ScriptableObject 中設定的 Delay）
        if (step.delay > 0)
        {
            yield return new WaitForSeconds(step.delay);
        }

        // 2. 處理回饋演出邏輯
        if (step.feedbackType == FeedbackType.Jumpscare)
        {
            // ★ 第一步：先觸發閃爍與音效
            // 這會啟動 FeedbackSystem 的 FlashRoutine，將螢幕填滿白色
            if (FeedbackSystem.Instance != null)
            {
                FeedbackSystem.Instance.Trigger(FeedbackType.Jumpscare);
            }

            // ★ 第二步：致盲等待 (Blinding Wait)
            // 等待 0.08 秒，這時玩家螢幕剛好是最亮的白光
            // 在白光遮蔽下切換圖片，玩家不會看到生硬的變換過程
            yield return new WaitForSeconds(0.08f);

            // ★ 第三步：顯示嚇人大圖
            if (jumpscareController != null)
            {
                jumpscareController.TriggerJumpscare(step.jumpscareImage);
            }
        }
        else
        {
            // 處理非 Jumpscare 的一般回饋（如警告紅閃、輕微震動等）
            if (FeedbackSystem.Instance != null)
            {
                FeedbackSystem.Instance.Trigger(step.feedbackType);
            }
        }

        // 3. 處理監視器畫面的 Override (覆蓋環境圖，例如螢幕突然變黑或出現鬼影)
        // 這部分邏輯獨立於 Jumpscare 大圖，會改變監視器本身的 Sprite
        if (step.overrideImage != null && step.triggerCamIndex >= 0)
        {
            // 記錄到 CamController 的 RuntimeOverrides 字典中，確保切換回來時圖還在
            camController.SetRuntimeOverride(step.triggerCamIndex, step.overrideImage);

            // 如果現在正在看的正是這台監視器，立刻手動更新畫面
            if (camController.currentCamIndex == step.triggerCamIndex)
            {
                camController.SetOverrideImage(step.overrideImage);
            }
        }
    }

    public void OnCheckListFinished()
    {
        if (decisionUI != null) decisionUI.SetActive(true);

        bool[] answers = checkListUI.GetAnswers();

        IDCardDefinition card = currentCharacter.idCard; // 你的 CharacterDefinition 有 idCard :contentReference[oaicite:1]{index=1}

        bool[] correctness = new bool[3];
        correctness[0] = (answers[0] == card.isValid);
        correctness[1] = (answers[1] == card.eyesNormal);
        correctness[2] = (answers[2] == card.teethNormal);

        // 這裡再丟進 Recorder
        if (GameSessionRecorder.Instance != null)
            GameSessionRecorder.Instance.SetChecklist(answers, correctness);

    }


    public void ConfirmDecision(bool approve)
    {
        if (InteractionLock.GlobalLock) return;

        // ★新增：記錄這一輪的入境/不入境與是否正確
        if (GameSessionRecorder.Instance != null)
            GameSessionRecorder.Instance.FinalizeDecision(approve);

        TutorialManager.Instance.AdvanceFromDecision();
        idCardUI.ClearCard();
        if (checkListUI != null) checkListUI.Deactivate();
        if (decisionUI != null) decisionUI.SetActive(false);

        StartNext();
    }


    void ShowEnding()
{
    SceneManager.LoadScene("ResultsScene");
}


//void OnEnable() { CamSwitchController.OnCamChanged += HandleCamChanged; }
//    void OnDisable() { CamSwitchController.OnCamChanged -= HandleCamChanged; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) StartNext();
    }
}