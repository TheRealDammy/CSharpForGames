using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statPointsText;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private PlayerStats stats;
    private ExperienceSystem exp;

    private void Start()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        exp = FindFirstObjectByType<ExperienceSystem>();
        Refresh();
    }

    public void Refresh()
    {
        statPointsText.text = $"Stat Points Available: {exp.GetStatPoints()}";

        healthText.text = $"Health Level: {stats.GetStatLevel(PlayerStatType.Health)}";
        attackText.text = $"Attack Level: {stats.GetStatLevel(PlayerStatType.Strength)}";
        durabilityText.text = $"Durability Level: {stats.GetStatLevel(PlayerStatType.Durability)}";
        staminaText.text = $"Stamina Level: {stats.GetStatLevel(PlayerStatType.Stamina)}";
    }
}
