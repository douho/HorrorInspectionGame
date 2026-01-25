using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using UnityEngine.InputSystem;


public class FocusManager : MonoBehaviour
{
    public static FocusManager Instance;   // 給其他 Script 存取用
    public static bool FocusLock = false;  // 控制 A/D 是否允許切換

    [Header("桌面可互動物件")]
    public IDCardUI idCardUI;
    public CheckListUI checkListUI;
    public ManualUI manualUI;

    private enum FocusTarget { IDcard, CheckList }
    private FocusTarget currentFocus = FocusTarget.IDcard;
    private bool stickXUsed = false;


    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //一開始先聚焦身分證
        SetFocus(FocusTarget.IDcard);
    }

    // Update is called once per frame
    void Update()
    {
        if (InteractionLock.DialogueLock) return;

        if (InteractionLock.GlobalLock) return; //鎖定中不進行焦點切換
        if (FocusLock) return;               // ★ 教學要求鎖住 A/D 時就不切換

        //左右方向鍵切換焦點
        if (Input.GetKeyDown(KeyCode.A)|| Input.GetKeyDown(KeyCode.LeftArrow))
            SwitchFocus(-1);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SwitchFocus(1);

        var pad = Gamepad.current;
        if (pad != null)
        {
            // D-pad：不用鎖
            if (pad.dpad.left.wasPressedThisFrame) SwitchFocus(-1);
            else if (pad.dpad.right.wasPressedThisFrame) SwitchFocus(1);

            // 左蘑菇頭：要方向鎖
            float x = pad.leftStick.x.ReadValue();

            if (!stickXUsed)
            {
                if (x < -0.6f) { SwitchFocus(-1); stickXUsed = true; }
                else if (x > 0.6f) { SwitchFocus(1); stickXUsed = true; }
            }

            // 放回中間才解鎖
            if (Mathf.Abs(x) < 0.2f) stickXUsed = false;
        }

    }

    void SwitchFocus(int dir) //dir:方向鍵direction 的簡稱
    {
        int next = ((int)currentFocus + dir +2 ) % 2;
        SetFocus((FocusTarget)next);
    }
    
    void SetFocus(FocusTarget target)
    {
        currentFocus = target;

        //呼叫每個 UI 的 Focus() 來顯示外框
        idCardUI.Focus(currentFocus == FocusTarget.IDcard);
        checkListUI.Focus(currentFocus == FocusTarget.CheckList);

        Debug.Log($"目前聚焦於 {currentFocus}");
    }

    //提供給其他程式重設焦點用
    public void ResetFocus() => SetFocus(FocusTarget.IDcard);
   
}
