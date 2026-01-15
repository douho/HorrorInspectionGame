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
            case 4: Step4_ReturnCam001(); break;
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
        dialogueManager.ShowDialogue("看來你就是新來的監察人員，接下來將一步步帶你進行檢查流程。\n請按空白鍵繼續。");
    }

    private void Step1_OpenID()
    {
        FocusManager.Instance?.ResetFocus();

        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = true;

        FocusManager.Instance?.ResetFocus();

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("首先，請按空白鍵打開身分證件，確認基本資料。");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step2_CheckID()
    {
        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = true;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請確保身分證頭貼與本人相符，且身分證尚在有效期限內。\n檢查完畢請按 ESC 關閉證件");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step3_GoToCam002()
    {
        InteractionLock.GlobalLock = false;
        FocusManager.FocusLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請用 Q/E 切換到下個監視器畫面，確認入關者眼睛狀況。\n為確保對象為人類，據觀察，「偽生物」的眼白會有紅色斑點。\n請多加注意。");
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step4_ReturnCam001()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("很好，再請切到下一個畫面。");
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
            "接著會進入清單檢查教學，請切回第一個畫面。"
        );

        // ★關鍵：Step5 不允許用空白鍵前進，避免空白鍵被拿去開 checklist 時偷跳 step
        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step6_OpenChecklist()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請用 A/D 切換到 Checklist 並按空白鍵開啟。");

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step7_ChecklistTick()
    {
        Debug.Log("Step7 Lock = " + InteractionLock.GlobalLock + ", DialogueLock = " + InteractionLock.DialogueLock);

        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue("請將所有檢查項目勾選完成。");

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step8_SendDecision()
    {
        InteractionLock.GlobalLock = false;

        dialogueManager.MoveToBottom();
        dialogueManager.ShowDialogue(
        "請按下「Submit」送出，接著選擇『允許』或『不允許』入境以完成審查。\n" +
        "你也可以先關閉清單，觀察後再決定。"
        );

        dialogueManager.nextKeyHint.SetActive(false);
    }

    private void Step9_EndTutorial()
    {
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
        if (step == 3 && camIndex == 1)
        {
            step = 4;
            GoToStep(step);
        }
        else if (step == 4 && camIndex == 0)
        {
            step = 5;
            GoToStep(step);
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
        // 教學步驟的空白鍵
        if (!TutorialFinished && dialogueManager.nextKeyHint.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            {
                step++;
                GoToStep(step);
            }
        }

        if (!TutorialFinished)
            CheckCameraProgress();

        // ★ 最後等待玩家按空白鍵結束教學
        if (waitingForTutorialEnd)
        {
            if (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
            {
                dialogueManager.HideDialogue();

                InteractionLock.GlobalLock = false;
                InteractionLock.DialogueLock = false;
                FocusManager.FocusLock = false;

                TutorialFinished = true;

                gameObject.SetActive(false); // ★ 完全關閉教學
                waitingForTutorialEnd = false;
            }
        }
    }

    private void CheckCameraProgress()
    {
        if (camController == null) return;

        int camIndex = camController.currentCamIndex;

        if (step == 3 && camIndex == 1)
        {
            step++;
            GoToStep(step);
        }
        else if (step == 4 && camIndex == 2)
        {
            step++;
            GoToStep(step);
        }
        else if (step == 5 && camIndex == 0)
        {
            step++;
            GoToStep(step);
        }
    }
}
