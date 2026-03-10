using System;
using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    //public TMP_Text sessionIdText;
    //public TMP_Text participantIdText;
    public TMP_Text buildVersion;
    public TMP_Text correct_answer;
    public TMP_Text wrong_answer;
    public TMP_Text totalTime;

    public TMP_Text uidDisplayText; // 拖入你要顯示 UID 的 UI 文字框

    void Start()
    {
        var r = GameSessionRecorder.Instance;
        if (r == null) return;

        buildVersion.text = r.feedbackCondition.ToString();
        correct_answer.text = r.CorrectCount().ToString();
        wrong_answer.text = r.WrongCount().ToString();

        float t = r.sessionElapsedSec;
        TimeSpan ts = TimeSpan.FromSeconds(t);
        totalTime.text = $"{ts.Minutes:00}:{ts.Seconds:00}";

        // 2. 【新增】呼叫上傳功能
        // 尋找場景中的 DataManager (GameDataUploader)
        GameDataUploader uploader = FindObjectOfType<GameDataUploader>();
        if (uploader != null)
        {
            uploader.UploadGameData(
                r.feedbackCondition.ToString(), // 遊玩版本 (以你的回饋等級當作版本)
                r.sessionElapsedSec,            // 遊玩時間
                r.CorrectCount(),               // 答對題數
                r.WrongCount()                  // 答錯題數
            );
        }

        if (uploader != null)
        {
            uidDisplayText.text = "ID: " + uploader.currentUID;
        }

    }
    public void BackToMainMenu()
    {
        Debug.Log("[ResultsUI] BackToMainMenu clicked");
        var r = GameSessionRecorder.Instance;
        Debug.Log("[ResultsUI] recorder is " + (r == null ? "NULL" : "OK"));

        if (r != null) r.ResetSession();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


}
