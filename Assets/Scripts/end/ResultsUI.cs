using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    //public TMP_Text sessionIdText;
    //public TMP_Text participantIdText;
    public TMP_Text buildVersionText;
    public TMP_Text correctText;
    public TMP_Text wrongText;

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

        buildVersionText.text = r.feedbackCondition.ToString();
        correctText.text = r.CorrectCount().ToString();
        wrongText.text = r.WrongCount().ToString();
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
