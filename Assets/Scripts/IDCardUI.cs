using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IDCardUI : MonoBehaviour
{
    public static event System.Action OnIDClose;
    public static event System.Action OnIDOpen; 
    [Header("通用小卡圖")]
    public Sprite commonClosedSprite;    // 桌面小卡通用圖片

    [Header("UI Reference")]
    public GameObject closedIconRoot; // 桌面上的小圖（身分證icon）
    public GameObject openViewRoot; // 放大檢視物件（身分證）
    public Image closedIconImage; // 小圖的 Image 元件
    public Image openImage; // 放大圖的 Image 元件
    public GameObject focusRing; // 聚焦時顯示外框
    public bool isFocused; // 是否被聚焦

    IDCardDefinition currentDef;
    bool isOpen;
    void Awake()
    {
        openViewRoot.SetActive(false);
        closedIconRoot.SetActive(false);
        focusRing?.SetActive(false);
    }

    // 外來者到場時，由流程控制器呼叫
    public void SetCard(IDCardDefinition def)
    {
        if (def != null && def.name == "EmptyIDCard") //只關閉 open/closed image，而不是整個卡 UI
        {
            closedIconRoot.SetActive(false);
            openViewRoot.SetActive(false);
            return;
        }

        currentDef = def;
        if (def != null && def.cardSprite != null)
        {
            // 小卡永遠用同一張
            closedIconImage.sprite = commonClosedSprite;

            // 放大卡才用角色專屬圖
            openImage.sprite = def.cardSprite;

            closedIconRoot.SetActive(true);
        }
        else
        {
            ClearCard();
        }
    }

    // 清空身分證（角色離場或換下一位）
    public void ClearCard()
    {
        currentDef = null;
        openViewRoot.SetActive(false);
        closedIconRoot.SetActive(false);
        isOpen = false;
        focusRing?.SetActive(false);
    }

    // 聚焦框顯示/隱藏
    public void Focus(bool on)
    {
        isFocused = on;
        focusRing?.SetActive(on);
    }

    // 打開放大檢視
    public void Activate()
    {
        if (currentDef == null)
        {
            Debug.LogWarning("Activate() → currentDef 為 null，沒辦法顯示大卡"); 
            return;
        }

        Debug.Log($"Activate() → 顯示角色 {currentDef.name} 的大卡");
        openViewRoot.SetActive(true);
        isOpen = true;

        // 教學模式需要用到這個事件
        OnIDOpen?.Invoke();
    }

    // 關閉放大檢視
    public void Deactivate()
    {
        if (!isOpen) return;

        openViewRoot.SetActive(false);
        isOpen = false;

        OnIDClose?.Invoke();
    }
    void Update()
    {
        if (InteractionLock.DialogueLock) return;

        if (InteractionLock.GlobalLock) return;
        //if(TutorialManager.currentStep == 6) return; // 教學模式第 6 步鎖住身分證操作
        if (!isFocused) return;

        // 空白鍵 / 搖桿 A：打開大卡
        if (!isOpen && (Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)))
        {
            Debug.Log("空白鍵被按下 → 嘗試打開大卡");
            Activate();
        }

        // R 鍵 / 搖桿 B：關閉大卡
        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)))
        {
            Debug.Log("R 鍵被按下 → 嘗試關閉大卡");
            Deactivate();
        }
    }

}
