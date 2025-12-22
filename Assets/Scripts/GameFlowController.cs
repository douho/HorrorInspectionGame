using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [Header("Data")]
    public CharacterDatabase characterDB; // 角色資料庫（把 ScriptableObject 拖進來）

    [Header("UI")]
    public IDCardUI idCardUI;// 身分證 UI（Canvas 裡的 IDCardUI）
    public GameObject checkListUI;// 檢查清單（Canvas 上的物件）
    public GameObject decisionUI;// 入境/不入境按鈕群組（Canvas 上的物件）
    public CamSwitchController camController; // 監視器切換 UI
    public TransitionManager transitionManager;

    private HashSet<int> triggeredCamSteps = new(); // 加一個「已執行過的 CAM Index 記錄」，避免同一張圖多次觸發 override。



    int currentIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // 遊戲開始，只關閉「放大檢視」按鈕關掉
        decisionUI.SetActive(false);

        //通知 CheckListUI 自己收起
        var list = checkListUI.GetComponent<CheckListUI>();
        if (list != null)
        {
            list.Deactivate(); // 關閉放大版
        }
        StartNext();
    }

    /// <summary>
    /// 換下一位外來者
    /// </summary>
    public void StartNext()
    {
        triggeredCamSteps.Clear();//清除 triggeredCamSteps： 新角色開始時，重置已觸發的 CAM 步驟記錄。

        if (TutorialManager.TutorialFinished)
        {
            // 正式遊戲 → 清空對話框，並停用 DialogueManager
            if (FindObjectOfType<DialogueManager>() != null)
                FindObjectOfType<DialogueManager>().HideDialogue();
        }

        // ★ 先把上一位的清單重置
        var list = checkListUI.GetComponent<CheckListUI>();
        if (list != null)
        {
            list.ClearCheckList();   // 清掉勾選
            list.Deactivate();       // 關閉放大清單與決策UI
        }

        currentIndex++;
        var ch = characterDB.GetByIndex(currentIndex);
        // ★ 第三位訪客自動 Jumpscare
        if (ch == null)
        {
            ShowEnding();
            return;
        }

        // 取代原本寫死的第三位訪客 Jumpscare
        TryTriggerJumpScare(ch, TriggerCondition.OnCamSwitch);

        // 顯示身分證（小卡固定、大卡換圖）
        idCardUI.SetCard(ch.idCard);
        if (camController != null)
            camController.SetCamImages(ch.monitorImages);
        // 檢查清單、決策按鈕保持關閉，等玩家操作後再開
        //if (checkListUI != null) checkListUI.SetActive(false);
        if (decisionUI != null) decisionUI.SetActive(false);

        // 強制監視器回到 CAM001
        CamSwitchController cam = FindObjectOfType<CamSwitchController>();
        cam.ForceSwitchTo(0);   // ★ 強制回到 CAM001（index = 0）

        Debug.Log($"StartNext: index={currentIndex}, character={ch?.displayName}, card={(ch?.idCard != null ? "OK" : "NULL")}");

    }
    public void OnCheckListFinished()
    {
        decisionUI.SetActive(true);
    }

    /// <summary>
    /// 玩家選擇入境 / 不入境
    /// </summary>
    public void ConfirmDecision(bool approve)
    {
        //if (TutorialManager.currentStep < 8) return;
        if (InteractionLock.isLocked) return;

        var ch = characterDB.GetByIndex(currentIndex);

        // 真實答案：faceMatches=true 且 expired=false 才合法
        bool legal = ch.idCard.faceMatches && !ch.idCard.expired;
        bool correct = (approve == legal);

        Debug.Log($"Decision: {(approve ? "Approve" : "Deny")} | Correct = {correct}");

        TutorialManager.Instance.AdvanceFromDecision();    // ★ 教學進入 Step9（新增）

        // 收尾
        idCardUI.ClearCard();
        checkListUI.GetComponent<CheckListUI>().Deactivate(); // 關閉檢查清單
        decisionUI.SetActive(false);

        StartNext();// 下一位
    }

    void ShowEnding()
    {
        Debug.Log("所有外來者結束 → 顯示結局");
    }
    private void TryTriggerJumpScare(CharacterDefinition ch, TriggerCondition condition)
    {
        if (ch.jumpScareSequence != null && ch.jumpScareSequence.triggerCondition == condition)
        {
            StartCoroutine(ExecuteJumpScare(ch.jumpScareSequence));
        }
    }

    private System.Collections.IEnumerator ExecuteJumpScare(JumpScareSequence seq)
    {
        foreach (var step in seq.steps)
        {
            yield return new WaitForSeconds(step.delay);
            FeedbackSystem.Instance.Trigger(step.feedbackType);
        }
    }

    private System.Collections.IEnumerator ExecuteStep(FeedbackStep step)
    {
        if (step.delay > 0)
            yield return new WaitForSeconds(step.delay);

        if (step.silenceAudio)
            FeedbackSystem.Instance.Mute();

        if (step.overrideImage != null)
            camController.SetOverrideImage(step.overrideImage);

        if (step.feedbackType == FeedbackType.Jumpscare)
        {
            Debug.Log("jumpscareImage 要播放的是：" + step.jumpscareImage?.name);
            FindObjectOfType<JumpscareController>()?.TriggerJumpscare(step.jumpscareImage);
        }
        else
        {
            FeedbackSystem.Instance.Trigger(step.feedbackType);
        }

    }


    void OnEnable()
    {
        CamSwitchController.OnCamChanged += HandleCamChanged;
    }
    void OnDisable()
    {
        CamSwitchController.OnCamChanged -= HandleCamChanged;
    }

    void HandleCamChanged(int camIndex)
    {
        var ch = characterDB.GetByIndex(currentIndex);
        var seq = ch.jumpScareSequence;

        if (ch == null) return;//當流程已結束時，CAM 切換也不會觸發任何錯誤
        if (seq == null) return;

        if (triggeredCamSteps.Contains(camIndex)) return;
        triggeredCamSteps.Add(camIndex);

        foreach (var step in seq.steps)
        {
            if (step.triggerCamIndex == camIndex)
            {
                StartCoroutine(ExecuteStep(step));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            StartNext();
        }
    }
}
