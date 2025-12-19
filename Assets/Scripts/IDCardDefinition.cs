using UnityEngine;

[CreateAssetMenu(menuName = "Game/ID Card Definition")]
public class IDCardDefinition : ScriptableObject
{
    [Header("Visuals")]
    public Sprite cardSprite; // 整張身分證的圖片（含所有文字資訊）

    [Header("Answer Key")] // 評分用的標準答案
    public bool faceMatches = true; // 身分證大頭貼是否符合臉部
    public bool expired = false; // 身分證是否過期（true = 已過期）

}
