using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClassData", menuName = "Player/PlayerClassData")]
public class CharacterClassData : ScriptableObject
{
    public CharacterClass characterClass;

    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseStamina = 100;
    public int baseStrength = 5;
    public int baseDurability = 0;

    [Header("Combat")]
    public CombatController combatPrefab;
}
