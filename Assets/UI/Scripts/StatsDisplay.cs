using TMPro;
using UnityEngine;

public class PlayerStatsDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;

    private PlayerStats stats;
    private PlayerHealth health;
    private TopDownCharacterController controller;
    private CombatController combatController;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
            Bind(player);
    }

    public void Bind(GameObject player)
    {
        stats = player.GetComponent<PlayerStats>();
        health = player.GetComponent<PlayerHealth>();
        controller = player.GetComponent<TopDownCharacterController>();

        stats.OnStatChanged += _ => Refresh();
        Refresh();
    }

    private void Refresh()
    {
        if (!stats) return;

        healthText.text = $"Health: {health.GetMaxHealth()}";
        staminaText.text = $"Stamina: {controller.GetMaxStamina()}";
        damageText.text = $"Damage: {combatController.GetDamage()}";
        durabilityText.text =
            $"Damage Reduction: {stats.GetDamageReductionPercent()}%";
        moveSpeedText.text = $"Move Speed: {controller.GetCurrentSpeed()}";
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnStatChanged -= _ => Refresh();
    }
}
