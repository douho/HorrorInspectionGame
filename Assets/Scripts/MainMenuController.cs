using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnLow;
    public Button btnMid;
    public Button btnHigh;
    public Button btnQuit;

    private void Start()
    {
        // 綁定按鈕事件
        btnLow.onClick.AddListener(() => StartGame(0));  // 低回饋
        btnMid.onClick.AddListener(() => StartGame(1));  // 中回饋
        btnHigh.onClick.AddListener(() => StartGame(2)); // 高回饋
        btnQuit.onClick.AddListener(QuitGame);
    }

    void StartGame(int level)
    {
        // ★ 重點：直接設定 FeedbackSystem 的靜態變數
        FeedbackSystem.FeedbackLevel = level;

        // 載入教學 / 主遊戲 Scene 名字（目前是 Tutorial）
        SceneManager.LoadScene("Tutorial");
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
