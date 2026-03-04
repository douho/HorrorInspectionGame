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

    void Start()
    {
        var r = GameSessionRecorder.Instance;
        //if (r == null)
        //{
        //    sessionIdText.text = "No Session";
        //    return;
        //}

        //sessionIdText.text = r.sessionId;
        //participantIdText.text = r.participantId;

        buildVersion.text = r.feedbackCondition.ToString();
        correct_answer.text = r.CorrectCount().ToString();
        wrong_answer.text = r.WrongCount().ToString();

        float t = r.sessionElapsedSec;
        TimeSpan ts = TimeSpan.FromSeconds(t);
        totalTime.text = $"{ts.Minutes:00}:{ts.Seconds:00}";

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
