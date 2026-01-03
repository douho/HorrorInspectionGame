using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CheckListUI;

public class CheckListUI : MonoBehaviour
{
    public static event System.Action OnChecklistOpen;
    public static event System.Action OnChecklistCompleted;
    public static event System.Action OnSubmitPressed;
    [Header("UI Reference")]
    public GameObject closedIconRoot; // 桌面上的小圖（清單icon）
    public GameObject openViewRoot; // 放大檢視物件（清單）
    public GameObject focusRing; // 聚焦框
    public GameObject submitBtn; // 提交表單按鈕
    public GameObject decisionUI; // 決策UI
    public GameFlowController gameFlow; // 遊戲流程控制器
    public bool isFocused; // 是否被聚焦

    [Header("檢查問題")]
    public QuestionUI[] questions; // 問題列表

    private bool isOpen; // 清單是否打開

    void Awake()
    {
        openViewRoot.SetActive(false);
        closedIconRoot.SetActive(true); // 小圖一直存在
        if (focusRing != null) focusRing.SetActive(false);
        if (submitBtn != null) submitBtn.SetActive(false);
    }

    public void Focus(bool on)
    {
        isFocused = on;
        focusRing?.SetActive(on);
    }

    public void Activate()
    {
        openViewRoot.SetActive(true);
        isOpen = true;

        OnChecklistOpen?.Invoke();
    }

    public void Deactivate()
    {
        openViewRoot.SetActive(false);
        isOpen = false;

        // 一起關閉入境/不入境 UI
        if (decisionUI != null)
            decisionUI.SetActive(false);
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
        if (!isOpen && (Input.GetKeyDown(KeyCode.Space)))
            Activate();
        if (isOpen && (Input.GetKeyDown(KeyCode.Escape)))
            Deactivate();
    }

    // Toggle 更新時檢查是否全填
    public void OnQuestionAnswered()
    {
        bool allAnswered = questions.All(q => q.HasAnswer);
        if (allAnswered && submitBtn != null)
            submitBtn.SetActive(true);

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
