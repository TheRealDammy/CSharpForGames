using UnityEngine;

public class PlayerBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        var stats = GetComponent<PlayerStats>();
        var health = GetComponent<PlayerHealth>();

        CharacterClass cls = CharacterSelectController.Instance != null
            ? CharacterSelectController.Instance.SelectedClass
            : CharacterClass.Swordsman;

        ApplyClass(cls, stats);
        SwapCombat(cls);
        health.ApplyStats(true);
    }

    private void ApplyClass(CharacterClass cls, PlayerStats stats)
    {
        switch (cls)
        {
            case CharacterClass.Swordsman:
                stats.SetBaseStats(120, 80, 10, 8);
                break;

            case CharacterClass.Archer:
                stats.SetBaseStats(90, 110, 8, 4);
                break;

            case CharacterClass.Mage:
                stats.SetBaseStats(70, 130, 12, 2);
                break;
        }
    }

    private void SwapCombat(CharacterClass cls)
    {
        foreach (var c in GetComponents<CombatController>())
            Destroy(c);

        switch (cls)
        {
            case CharacterClass.Swordsman:
                gameObject.AddComponent<SwordsmanCombatController>();
                break;

            case CharacterClass.Archer:
                gameObject.AddComponent<ArcherCombatController>();
                break;

            case CharacterClass.Mage:
                gameObject.AddComponent<MageCombatController>();
                break;
        }
    }
}
