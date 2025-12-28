using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CamSwitchController : MonoBehaviour
{
    public static event System.Action<int> OnCamChanged; // 新增：當監視器畫面切換時觸發，參數為目前的 CAM 索引
    public Image camDisplay; // 用來顯示監視器畫面
    public Button btnPrev, btnNext; // 左右切換按鈕

    public int currentCamIndex = 0; // 當前顯示的 CAM 索引，預設為 CAM001
    private Sprite[] camImages; // 三張 CAM 畫面

    void Start()
    {
        //UpdateCamView();

        // 設定按鈕點擊事件
        btnPrev.onClick.AddListener(() => ChangeCam(-1));
        btnNext.onClick.AddListener(() => ChangeCam(1));
    }
    // Assets/Scripts/CamSwitchController.cs
    public void SetCamImages(Sprite[] images, CharacterDefinition ch)
    {
        camImages = images;
        currentCamIndex = Mathf.Clamp(currentCamIndex, 0, camImages.Length - 1);
        UpdateCamView();
    }



    // Update is called once per frame
    void Update()
    {
        if (InteractionLock.DialogueLock) return;

        if (InteractionLock.isLocked) return;
        //if (TutorialManager.currentStep == 6) return;
        
        //支援鍵盤方向鍵切換
        if (Input.GetKeyDown(KeyCode.Q))
            ChangeCam(-1);
        else if(Input.GetKeyDown(KeyCode.E))
            ChangeCam(1);
    }
    void ChangeCam(int direction)
    {
        currentCamIndex += direction;

        //限制只能在範圍內切換
        currentCamIndex = Mathf.Clamp(currentCamIndex, 0, camImages.Length - 1);

        UpdateCamView();

        // 修正後：在切換後再觸發事件
        OnCamChanged?.Invoke(currentCamIndex); // 觸發事件，通知監聽者目前的 CAM 索引

    }
    public void ForceSwitchTo(int index)
    {
        if (camImages == null || camImages.Length == 0)
        {
            Debug.LogWarning("ForceSwitchTo() 失敗：camImages 尚未設定");
            return;
        }

        currentCamIndex = Mathf.Clamp(index, 0, camImages.Length - 1);
        UpdateCamView();

        // ★ 強制切換也要觸發 OnCamChanged，否則 Tutorial 的進度會不會跑
        OnCamChanged?.Invoke(currentCamIndex);
    }

    //支援覆蓋 CAM 畫面
    public void SetOverrideImage(Sprite overrideSprite) 
    {
        if (camDisplay != null && overrideSprite != null)
            camDisplay.sprite = overrideSprite;
    }

    void UpdateCamView() 
    {
        //Debug.Log($"[CamSwitch] UpdateCamView() 目標Image={camDisplay?.name}，要設的圖={camImages?[currentCamIndex]?.name}", this);
        //if (camDisplay == null || camImages == null || camImages.Length == 0) return;
        //切換監視器畫面
        camDisplay.sprite = camImages[currentCamIndex];

        //// 讓你在 Inspector 也能立刻看到有沒有被設上
        //Debug.Log($"[CamSwitch] 已指定 sprite：{camDisplay.sprite?.name} 給 {camDisplay.name}", this);

        // 控制按鈕顯示（只在中間畫面時出現兩顆）
        btnPrev.gameObject.SetActive(currentCamIndex > 0);  // 只有不是最左邊才顯示左鍵
        btnNext.gameObject.SetActive(currentCamIndex < camImages.Length - 1); // 只有不是最右邊才顯示右鍵
    }
}
