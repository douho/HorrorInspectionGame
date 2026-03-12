using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// 這定義了我們要傳送的資料格式
[System.Serializable]
public class GameData
{
    public string uid;
    public string version;
    public float playTime;
    public int correctCount;
    public int wrongCount;
}


public class GameDataUploader : MonoBehaviour
{
    // 把剛才 Google 給你的 URL 貼在這邊
    string url = "https://script.google.com/macros/s/AKfycbx6plsd9Yco8luTGkHeKPFtlmYl-ZMAz5yXEt2P0oREh_AipboN1BUhKoMBTG-OrVms/exec";

    public string currentUID;

    void Awake()
    {
        // 檢查是否已經有另一個 DataManager 存在（避免重複）
        var objs = GameObject.FindGameObjectsWithTag("DataManager");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // 這行最重要：切換場景時不銷毀
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.tag = "DataManager"; // 記得在 Unity Editor 幫這個物件設 Tag
        }
    }
    void Start()
    {
        // 產生 6 位純數字 ID
        currentUID = UnityEngine.Random.Range(1000, 9999).ToString();
        Debug.Log("本次測試 UID: " + currentUID);
    }

    // 當遊戲結束時調用這個 Function
    public void UploadGameData(string ver, float time, int correct, int wrong)
    {
        GameData data = new GameData
        {
            uid = currentUID,
            version = ver,
            playTime = time,
            correctCount = correct,
            wrongCount = wrong
        };
        StartCoroutine(PostData(JsonUtility.ToJson(data)));
    }

    IEnumerator PostData(string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 關鍵：從雲端拿回正式的流水號 (001, 002...)
            currentUID = request.downloadHandler.text;
            Debug.Log("【雲端後台】上傳成功！分配到的 ID 是: " + currentUID);

            // 這裡可以呼叫 UI 更新，讓玩家看到正式 ID
            if (FindObjectOfType<ResultsUI>() != null)
            {
                // 假設你在 ResultsUI 寫了一個更新文字的 Function
                FindObjectOfType<ResultsUI>().UpdateUIDDisplay(currentUID);
            }
        }
    }
}