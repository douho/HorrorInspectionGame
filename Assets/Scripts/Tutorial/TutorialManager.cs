using UnityEngine;
using UnityEngine.InputSystem;


public class TutorialManager : MonoBehaviour
{
    public CamSwitchController camController;
    public static TutorialManager Instance;
    public DialogueManager dialogueManager;

    public static bool TutorialFinished = false;

    private int step = 0;
    private bool waitingForTutorialEnd = false;

    public TutorialSpotlight spotlight;
    public ManualUI manualUI; // 讓你能抓 closedIconRoot


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        // ★ 教學已完成 → 完全停用
        if (TutorialFinished)
        {
            gameObject.SetActive(false);
            return;
        }

        step = 0;
        GoToStep(step);
    }

    private System.Collections.IEnumerator ConsumeOneFrame()
    {
        // 暫時鎖住，讓空白鍵不會穿透去開 ID/Manual/Checklist
        bool prevGlobal = InteractionLock.GlobalLock;
        bool prevDialogue = InteractionLock.DialogueLock;

        InteractionLock.GlobalLock = true;
        InteractionLock.DialogueLock = true;

        yield return null;

        InteractionLock.GlobalLock = prevGlobal;
        InteractionLock.DialogueLock = prevDialogue;
    }

    public void GoToStep(int s)
    {
        if (TutorialFinished) return; // ★ 若已完成則不再執行任何步驟

        step = s;

        switch (s)
        {
            case 0: Step0_Welecome(); break;
            case 1: Step1_OpenID(); break;
            case 2: Step2_CheckID(); break;
            case 3: Step3_GoToCam002(); break;
            case 4: Step4_GoToCam003(); break;
            case 5: Step5_TeethHint(); break;
            case 6: Step6_OpenChecklist(); break;
            case 7: Step7_ChecklistTick(); break;
            case 8: Step8_SendDecision(); break;
            case 9: Step9_EndTutorial(); break;
        }
    }

    private void Step0_Welecome()
    {
        InteractionLock.GlobalLock = true;
        FocusManager.FocusLock = true;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("看來你就是新來的監察人員，接下來將一步步帶你進行檢查流程。\n請按 【 ○ 】 繼續。");
    }

    private void Step1_OpenID()
    {
        FocusManager.Instance?.ResetFocus();

        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = true;

        FocusManager.Instance?.ResetFocus();

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("首先，請按 【 ○ 】 打開身分證件，確認基本資料。");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step2_CheckID()
    {
        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = true;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請確保身分證頭貼與本人相符，且身分證尚在有效期限內。\n檢查完畢請按 【 × 】 關閉證件");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step3_GoToCam002()
    {
        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = false;
        InteractionLock.CameraLock = false;  // 這行是關鍵

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("接下來，為確保對象為人類，\n研究指出多數「偽生物」的眼白會有紅色斑點，請多加注意。\n請用 【 R1 】 切換到下個監視器畫面。");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step4_GoToCam003()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("很好，再請用 【 R1 】 切到下一個畫面。");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step5_TeethHint()
    {
        //InteractionLock.GlobalLock = false;
        InteractionLock.GlobalLock = false;
        InteractionLock.CameraLock = false;


        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue(
            "除了眼睛，也請注意牙齒是否「過度潔白整齊」\n" +
            "接著會進入清單檢查教學，請用 【 L1 】 切回第一個畫面。"
        );

        // ★關鍵：Step5 不允許用空白鍵前進，避免空白鍵被拿去開 checklist 時偷跳 step
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step6_OpenChecklist()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請用 【 左下類比搖桿 】 切換到 【右側檢查清單】 並按 【 ○ 】 開啟。");

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step7_ChecklistTick()
    {
        Debug.Log("Step7 Lock = " + InteractionLock.GlobalLock + ", DialogueLock = " + InteractionLock.DialogueLock);

        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請利用 【 左下類比搖桿 】 及 【 ○ 】 將所有檢查項目勾選完成。");

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step8_SendDecision()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue(
        "請按下 【 Submit 】 送出\n" +
        "你也可以先按 【 × 】 關閉清單，觀察後再決定。"
        );

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step9_EndTutorial()
    {
        if (spotlight != null && manualUI != null && manualUI.closedIconRoot != null)
        {
            var rt = manualUI.closedIconRoot.GetComponent<RectTransform>();
            spotlight.Show(rt);
        }

        // 只在最後一步，把手冊提示亮起
        if (FocusManager.Instance != null && FocusManager.Instance.manualUI != null)
        {
            FocusManager.Instance.manualUI.SetHintGlow(true);
        }

        InteractionLock.DialogueLock = true;
        InteractionLock.GlobalLock = true;
        FocusManager.FocusLock = true;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue(
            "做得不錯，新人。若有需要，可查看左側的手冊。\n" +
            "記住……「偽生物」擅長模仿，你的每一個判斷，都會影響到城市的安全。\n" +
            "保持警覺吧。"
        );

        waitingForTutorialEnd = true;
    }

    private void OnEnable()
    {
        IDCardUI.OnIDOpen += HandleIDOpen;
        IDCardUI.OnIDClose += HandleIDClosed;
        CamSwitchController.OnCamChanged += HandleCamChanged;
        CheckListUI.OnChecklistOpen += HandleChecklistOpen;
        CheckListUI.OnChecklistCompleted += HandleChecklistCompleted;
        CheckListUI.OnSubmitPressed += HandleSubmitPressed;
    }

    private void OnDisable()
    {
        IDCardUI.OnIDOpen -= HandleIDOpen;
        IDCardUI.OnIDClose -= HandleIDClosed;
        CamSwitchController.OnCamChanged -= HandleCamChanged;
        CheckListUI.OnChecklistOpen -= HandleChecklistOpen;
        CheckListUI.OnChecklistCompleted -= HandleChecklistCompleted;
        CheckListUI.OnSubmitPressed -= HandleSubmitPressed;
    }

    private void HandleIDOpen()
    {
        if (step != 1) return;

        step++;
        GoToStep(step);
    }

    private void HandleIDClosed()
    {
        if (step != 2) return;

        step++;
        GoToStep(step);
    }

    private void HandleCamChanged(int camIndex)
    {
        // step3: 切到 CAM002 (1)
        if (step == 3 && camIndex == 1)
        {
            step = 4;
            GoToStep(step);
            return;
        }

        // step4: 必須切到 CAM003 (2)
        if (step == 4 && camIndex == 2)
        {
            step = 5;
            GoToStep(step);
            return;
        }

        // step5: 必須切回 CAM001 (0)
        if (step == 5 && camIndex == 0)
        {
            step = 6;
            GoToStep(step);
            return;
        }
    }

    private void HandleChecklistOpen()
    {
        if (step != 6) return;

        step = 7;
        GoToStep(step);
    }

    private void HandleChecklistCompleted()
    {
        if (step != 7) return;

        step = 8;
        GoToStep(step);
    }

    private void HandleSubmitPressed()
    {
        if (step != 8) return;

        Debug.Log("Submit 被按下（教學模式 Step8）");
    }

    public void AdvanceFromDecision()
    {
        if (TutorialFinished) return;

        step = 9;
        GoToStep(step);
    }

    private void Update()
    {
        // 教學關卡第一個按鍵 → 啟動計時（只會啟動一次）
        if (!TutorialFinished)
        {
            if (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            {
                if (GameSessionRecorder.Instance != null)
                    GameSessionRecorder.Instance.StartSessionTimerIfNeeded();
            }
        }

        // 教學步驟的空白鍵
        if (!TutorialFinished && dialogueManager.nextKeyHint.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            {
                StartCoroutine(ConsumeOneFrame());   // 關鍵：消耗一幀
                step++;
                GoToStep(step);
            }
        }

        //if (!TutorialFinished)
        //    CheckCameraProgress();

        // ★ 最後等待玩家按空白鍵結束教學
        if (waitingForTutorialEnd)
        {
            if (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            {
                //// 先鎖住，避免同一顆鍵被 Checklist 同幀吃掉
                //InteractionLock.GlobalLock = true;

                dialogueManager.HideDialogue();

                if (spotlight != null) spotlight.Hide(); // 這裡才關掉 spotlight

                // 不要在這裡把 ring 關掉，改成「切回正常聚焦到手冊」
                if (FocusManager.Instance != null)
                {
                    // 先把教學用強制 glow 關掉（避免之後跟 Focus() 打架）
                    if (FocusManager.Instance.manualUI != null)
                        FocusManager.Instance.manualUI.SetHintGlow(false);

                    // 強制把焦點指定在手冊（玩家立刻知道下一步）
                    FocusManager.Instance.FocusManual();
                }

                //// 把手冊提示燈熄掉（避免一直亮）
                //if (FocusManager.Instance != null && FocusManager.Instance.manualUI != null)
                //    FocusManager.Instance.manualUI.SetHintGlow(false);
                TutorialFinished = true;

                // 用 coroutine 延後解鎖，避免同幀空白鍵穿透去開手冊
                StartCoroutine(EndTutorialRoutine());

                // 解鎖
                //InteractionLock.GlobalLock = false;
                //InteractionLock.DialogueLock = false;
                //FocusManager.FocusLock = false;

                // ★新增：教學結束後，把正常焦點指定到手冊（讓 ring 留下來）
                //FocusManager.Instance?.FocusManual();

                //gameObject.SetActive(false);
                waitingForTutorialEnd = false;

            }
        }

    }
    //private System.Collections.IEnumerator EndTutorialRoutine()
    //{
    //    yield return null;                 // 等一幀，讓結束鍵不會同幀觸發 Checklist
    //    InteractionLock.GlobalLock = false; // 解鎖，恢復操作
    //    gameObject.SetActive(false);        // 最後再關掉 Tutorial
    //}

    private System.Collections.IEnumerator EndTutorialRoutine()
    {
        // 這一幀先維持鎖住，讓空白鍵不會被 Manual/Checklist/IDCard 吃到
        yield return null;

        InteractionLock.GlobalLock = false;
        InteractionLock.DialogueLock = false;
        FocusManager.FocusLock = false;

        gameObject.SetActive(false);
    }


}
