using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CheckListUI;
using UnityEngine.InputSystem;


public class CheckListUI : MonoBehaviour
{
    public static event System.Action OnChecklistOpen;
    public static event System.Action OnChecklistCompleted;
    public static event System.Action OnSubmitPressed;
    [Header("UI Reference")]
    public GameObject closedIconRoot; // 桌面上的小圖（清單icon）
    public GameObject openViewRoot; // 放大檢視物件（清單）
    //public GameObject focusRing; // 聚焦框
    public GameObject focusRingDesk;   // 桌面聚焦框（可選：如果你還要在 checklist icon 上顯示）
    public GameObject focusRingCheck;  // checklist 導覽用聚焦框（FocusRing_Check）

    public GameObject submitBtn; // 提交表單按鈕
    public GameObject decisionUI; // 決策UI
    public GameFlowController gameFlow; // 遊戲流程控制器
    public bool isFocused; // 是否被聚焦

    [Header("檢查問題")]
    public QuestionUI[] questions; // 問題列表

    private bool isOpen; // 清單是否打開

    // === Gamepad 導覽狀態（Checklist 模式）===
    private int qIndex = 0;          // 目前第幾題 (0~2)
    private int colIndex = 0;        // 0=是(okToggle) / 1=否(badToggle)

    // 蘑菇頭方向鎖（避免連發）
    private bool stickXUsed = false;
    private bool stickYUsed = false;

    // 可調門檻
    private const float StickPressThreshold = 0.6f;
    private const float StickReleaseThreshold = 0.2f;


    void Awake()
    {
        openViewRoot.SetActive(false);
        closedIconRoot.SetActive(true); // 小圖一直存在
        if (focusRingCheck != null) focusRingCheck.SetActive(false);
        if (submitBtn != null) submitBtn.SetActive(false);
    }

    public void Focus(bool on)
    {
        isFocused = on;

        // 桌面聚焦框要不要顯示取決於你有沒有用 focusRingDesk
        if (focusRingDesk != null)
            focusRingDesk.SetActive(on && !isOpen);

        // checklist ring 只在 checklist 開著時才顯示
        if (focusRingCheck != null)
            focusRingCheck.SetActive(on && isOpen);
    }


    private void UpdateFocusRingByCursor()
    {
        if (focusRingCheck == null) return;
        if (questions == null || questions.Length == 0) return;

        var q = questions[Mathf.Clamp(qIndex, 0, questions.Length - 1)];
        Toggle t = (colIndex == 0) ? q.okToggle : q.badToggle;
        if (t == null) return;

        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;

        // 最穩的方式：直接把 ring 的位置貼到 toggle 上
        focusRingCheck.transform.position = rt.position;
                
        // 如果你覺得 ring 尺寸要跟 toggle 一樣，可加這行（可選）
        // focusRing.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
    }

    public void Activate()
    {
        openViewRoot.SetActive(true);
        isOpen = true;

        // 進入 checklist 模式：鎖桌面 A/D 切換
        FocusManager.FocusLock = true;

        // 重置導覽狀態
        qIndex = 0;
        colIndex = 0;
        stickXUsed = false;
        stickYUsed = false;

        OnChecklistOpen?.Invoke();
        if (focusRingDesk != null) focusRingDesk.SetActive(false);
        if (focusRingCheck != null) focusRingCheck.SetActive(true);
        UpdateFocusRingByCursor();
    }

    public void Deactivate()
    {
        openViewRoot.SetActive(false);
        isOpen = false;

        // 一起關閉入境/不入境 UI
        if (decisionUI != null)
            decisionUI.SetActive(false);

        FocusManager.FocusLock = false;
        if (focusRingCheck != null) focusRingCheck.SetActive(false);
        if (focusRingDesk != null) focusRingDesk.SetActive(isFocused); // 回到桌面焦點狀態


    }

    public bool[] GetAnswers()
    {
        // Q0 身分證尚未過期、Q1 眼睛正常、Q2 牙齒正常
        bool[] answers = new bool[3];

        answers[0] = questions[0].okToggle.isOn; // 是
        answers[1] = questions[1].okToggle.isOn;
        answers[2] = questions[2].okToggle.isOn;

        return answers;
    }

    private void CheckAllCompleted()
    {
        // 用問題陣列檢查每題是否有選擇
        foreach(var q in questions)
        {
            if (!q.HasAnswer)
                return; // 有一題沒選擇就直接返回
        }
        // 全部題目都有勾選（ok 或 bad 都算）
        OnChecklistCompleted?.Invoke();
    }
    void Update()
    {
        if (InteractionLock.DialogueLock) return;
        if (InteractionLock.isLocked) return;
        if (!isFocused) return;

        // 沒開時按 Space / ○ 打開
        if (!isOpen && (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)))
        {
            Activate();
            UpdateFocusRingByCursor(); 
            return;
        }

        if (!isOpen) return;

        // 開著的時候：每幀都吃導覽
        HandleChecklistGamepadNavigation();

        // 開著時按 ESC / × 關掉
        if (Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            Deactivate();
            return;
        }

        //if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)))
        //{
        //    Deactivate();
        //    HandleChecklistGamepadNavigation();
        //}
    }
    private void HandleChecklistGamepadNavigation()
    {
        var pad = Gamepad.current;
        if (pad == null) return;

        // --- 1) 上下：換題目（D-pad 直接用 pressedThisFrame；蘑菇頭用方向鎖）---
        if (pad.dpad.up.wasPressedThisFrame) MoveQuestion(-1);
        else if (pad.dpad.down.wasPressedThisFrame) MoveQuestion(1);

        float y = pad.leftStick.y.ReadValue();
        if (!stickYUsed)
        {
            if (y > StickPressThreshold) { MoveQuestion(-1); stickYUsed = true; }
            else if (y < -StickPressThreshold) { MoveQuestion(1); stickYUsed = true; }
        }
        if (Mathf.Abs(y) < StickReleaseThreshold) stickYUsed = false;

        // --- 2) 左右：切換 是/否 ---
        if (pad.dpad.left.wasPressedThisFrame) MoveColumn(-1);
        else if (pad.dpad.right.wasPressedThisFrame) MoveColumn(1);

        float x = pad.leftStick.x.ReadValue();
        if (!stickXUsed)
        {
            if (x < -StickPressThreshold) { MoveColumn(-1); stickXUsed = true; }
            else if (x > StickPressThreshold) { MoveColumn(1); stickXUsed = true; }
        }
        if (Mathf.Abs(x) < StickReleaseThreshold) stickXUsed = false;

        bool allAnswered = questions.All(qq => qq.HasAnswer);
        // --- 3) ○ 確認：把目前格子打勾（Toggle 互斥你已 WireExclusive 會處理）---
        if (pad.buttonEast.wasPressedThisFrame)
        {
            if (allAnswered && submitBtn != null && submitBtn.activeSelf)
                SubmitForm();
            else
                ApplyAnswerByCursor();

        }
        // --- 4) 如果三題都填完，允許 ○ 直接 Submit（不用再用滑鼠點按鈕）---
            // 你也可以改成「按一次○先把 submit 高亮，再按一次○送出」
            // 這裡先用最簡單：allAnswered 時再按一次 ○ 就送出
    }

    private void MoveQuestion(int delta)
    {
        qIndex = Mathf.Clamp(qIndex + delta, 0, questions.Length - 1);
        // 你如果有要做「小 focusRing」可以從這裡更新位置
        UpdateFocusRingByCursor();

    }

    private void MoveColumn(int delta)
    {
        colIndex = (colIndex + delta) < 0 ? 1 : (colIndex + delta) > 1 ? 0 : (colIndex + delta);
        // 你如果有要做「小 focusRing」可以從這裡更新位置
        UpdateFocusRingByCursor();

    }

    private void RefreshSubmitVisibility()
    {
        if (submitBtn == null || questions == null) return;

        bool allAnswered = questions.All(q => q.HasAnswer);
        submitBtn.SetActive(allAnswered);
    }

    // Toggle 更新時檢查是否全填
    public void OnQuestionAnswered()
    {
        RefreshSubmitVisibility();
        CheckAllCompleted();
    }

    // 玩家點擊提交表單
    public void SubmitForm()
    {
        Debug.Log("Submit 按鈕被按下了！");

        
        OnSubmitPressed?.Invoke();// 教學事件：讓 TutorialManager 知道 Submit 被按下

        gameFlow.OnCheckListFinished();//開啟入境/不入境 UI
        //if (submitBtn != null)
        //    submitBtn.SetActive(false);

    }

    // 清空表單（每位角色結束後）
    public void ClearCheckList()
    {
        foreach (var q in questions)
            q.Clear();
        if (submitBtn != null)
            submitBtn.SetActive(false);
    }

    void Start()
    {
        // 啟動時綁定 Toggle
        foreach (var q in questions)
        {
            q.WireExclusive(this);
        }
    }
    private void ApplyAnswerByCursor()
    {
        if (questions == null || questions.Length == 0) return;

        var q = questions[Mathf.Clamp(qIndex, 0, questions.Length - 1)];

        bool chooseYes = (colIndex == 0);

        if (q.okToggle != null) q.okToggle.SetIsOnWithoutNotify(chooseYes);
        if (q.badToggle != null) q.badToggle.SetIsOnWithoutNotify(!chooseYes);

        // 手動通知一次（讓 submit 的顯示更新）
        OnQuestionAnswered();
    }

    [System.Serializable]
    public class QuestionUI
    {
        public Toggle okToggle;
        public Toggle badToggle;

        public bool HasAnswer => okToggle.isOn || badToggle.isOn;

        public void WireExclusive(CheckListUI parent)
        {
            okToggle.onValueChanged.AddListener(on =>
            {
                if (on) badToggle.isOn = false;
                parent.OnQuestionAnswered();
            });
            badToggle.onValueChanged.AddListener(on =>
            {
                if (on) okToggle.isOn = false;
                parent.OnQuestionAnswered();
            });
        }
        public void Clear()
        {
            okToggle.isOn = false;
            badToggle.isOn = false;
        }
    }
}
