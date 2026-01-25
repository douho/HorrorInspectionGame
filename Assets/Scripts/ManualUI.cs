using UnityEngine;
using UnityEngine.InputSystem;

public class ManualUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject closedIconRoot;  // 桌面手冊小圖
    public GameObject openViewRoot;    // 手冊放大頁
    public GameObject focusRingDesk;   // 手冊桌面 FocusRing（或提示框）

    public bool isFocused;
    private bool isOpen;

    void Awake()
    {
        if (openViewRoot != null) openViewRoot.SetActive(false);
        if (closedIconRoot != null) closedIconRoot.SetActive(true);
        if (focusRingDesk != null) focusRingDesk.SetActive(false);
    }

    public void Focus(bool on)
    {
        isFocused = on;
        // 沒開手冊時才顯示桌面 ring（跟你 CheckListUI 的邏輯一致）
        if (focusRingDesk != null) focusRingDesk.SetActive(on && !isOpen);
    }

    // 教學用：強制亮起（不必真的聚焦也能亮）
    public void SetHintGlow(bool on)
    {
        if (focusRingDesk != null) focusRingDesk.SetActive(on);
    }

    public void Activate()
    {
        if (openViewRoot != null) openViewRoot.SetActive(true);
        isOpen = true;
        if (focusRingDesk != null) focusRingDesk.SetActive(false);
    }

    public void Deactivate()
    {
        if (openViewRoot != null) openViewRoot.SetActive(false);
        isOpen = false;
        // 回到桌面時如果仍聚焦就亮
        if (focusRingDesk != null) focusRingDesk.SetActive(isFocused);
    }

    void Update()
    {
        // 只要 Tutorial/Dialogue 在鎖，就不要讓空白鍵穿透去開手冊
        if (InteractionLock.DialogueLock) return;
        if (InteractionLock.GlobalLock) return;
        if (!isFocused) return;

        var pad = Gamepad.current;

        // 空白鍵/○ 打開
        if (!isOpen && (Input.GetKeyDown(KeyCode.Space) || (pad != null && pad.buttonEast.wasPressedThisFrame)))
            Activate();

        // ESC/× 關掉
        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || (pad != null && pad.buttonSouth.wasPressedThisFrame)))
            Deactivate();
    }
}
