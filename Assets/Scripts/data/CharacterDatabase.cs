using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterDefinition> entries = new();

    public CharacterDefinition GetByIndex(int i)
    {
        if (i < 0 || i >= entries.Count) return null;
        return entries[i];
    }
}
