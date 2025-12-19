using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    public int characterId; // 角色的唯一識別碼
    public string displayName;
    public Sprite protrait; // 角色的頭像，可留空
    public IDCardDefinition idCard; // 角色對應的身分證

    [Header("監視器畫面")]
    public Sprite[] monitorImages = new Sprite[3]; // 三張監視器畫面
}
