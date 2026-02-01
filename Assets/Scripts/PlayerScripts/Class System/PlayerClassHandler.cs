using UnityEngine;

public class PlayerClassHandler : MonoBehaviour
{
    public CharacterClassData SelectedClass { get; private set; }
    public static PlayerClassHandler Instance { get; private set; }

    private PlayerStats stats;
    private TopDownCharacterController movement;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<TopDownCharacterController>();
    }

    public void ApplyClass(CharacterClassData classData)
    {
        SelectedClass = classData;

        // Apply base stats
        stats.SetBaseStats(
            classData.baseHealth,
            classData.baseStamina,
            classData.baseStrength,
            classData.baseDurability
        );

        // Remove old combat
        foreach (var c in GetComponents<CombatController>())
            Destroy(c);

        // ADD combat dynamically
        CombatController combat =
            Instantiate(classData.combatPrefab, transform);

        combat.transform.localPosition = Vector3.zero;
    }
}
