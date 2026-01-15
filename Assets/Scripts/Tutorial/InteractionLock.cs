using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractionLock
{
    // 真的需要整體禁止輸入時用（例如換訪客過場）
    public static bool GlobalLock = false;

    // 只鎖監視器切換（避免某些流程禁止切 CAM）
    public static bool CameraLock = false;

    // 你原本就有：對話中鎖定
    public static bool DialogueLock = false;
}
