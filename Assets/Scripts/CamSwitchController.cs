using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class CamSwitchController : MonoBehaviour
{
    public static event System.Action<int> OnCamChanged; // 新增：當監視器畫面切換時觸發，參數為目前的 CAM 索引
    public Image camDisplay; // 用來顯示監視器畫面
    public Button btnPrev, btnNext; // 左右切換按鈕

    public int currentCamIndex = 0; // 當前顯示的 CAM 索引，預設為 CAM001
    private Sprite[] camImages; // 三張 CAM 畫面

    private Sprite[] camSprites;  // 你 SetCamImages 傳進來的角色預設圖
    private readonly Dictionary<int, Sprite> runtimeOverrides = new Dictionary<int, Sprite>();

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
        camSprites = images;
        runtimeOverrides.Clear();          // ★重要：換角色就清掉 override
        RefreshCurrentCamFromData();       // ★重要：立即刷新顯示成預設圖
    }

    public void SetRuntimeOverride(int camIndex, Sprite sprite)
    {
        if (sprite == null) return;
        runtimeOverrides[camIndex] = sprite;

        // 如果玩家剛好在這個 CAM，立刻換畫面
        if (currentCamIndex == camIndex)
            camDisplay.sprite = sprite;
    }

    public void ClearRuntimeOverrides()
    {
        runtimeOverrides.Clear();
    }

    public void RefreshCurrentCamFromData()
    {
        if (camSprites == null || camSprites.Length == 0) return;

        if (runtimeOverrides.TryGetValue(currentCamIndex, out var s) && s != null)
            camDisplay.sprite = s;
        else
            camDisplay.sprite = camSprites[currentCamIndex];
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

        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.leftShoulder.wasPressedThisFrame) ChangeCam(-1);   // L1
            else if (pad.rightShoulder.wasPressedThisFrame) ChangeCam(1); // R1
        }

    }
    public void ChangeCam(int delta)
    {
        if (camSprites == null || camSprites.Length == 0) return;

        currentCamIndex += delta;
        if (currentCamIndex < 0) currentCamIndex = camSprites.Length - 1;
        if (currentCamIndex >= camSprites.Length) currentCamIndex = 0;

        RefreshCurrentCamFromData();              
        OnCamChanged?.Invoke(currentCamIndex);
    }

    public void ForceSwitchTo(int index, bool invokeEvent = true)
    {
        if (camSprites == null || camSprites.Length == 0) return;

        currentCamIndex = Mathf.Clamp(index, 0, camSprites.Length - 1);

        RefreshCurrentCamFromData();              // ★改這個
        if (invokeEvent) OnCamChanged?.Invoke(currentCamIndex);
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
