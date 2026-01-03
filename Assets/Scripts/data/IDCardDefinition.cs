using UnityEngine;

[CreateAssetMenu(menuName = "Game/ID Card Definition")]
public class IDCardDefinition : ScriptableObject
{
    [Header("Visuals")]
    public Sprite cardSprite; // 整張身分證的圖片（含所有文字資訊）

    [Header("Answer Key")] // 評分用的標準答案
    public bool isValid;      // 身分證尚未過期
    public bool eyesNormal;   // 眼睛正常
    public bool teethNormal;  // 牙齒正常
}
