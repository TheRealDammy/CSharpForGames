using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Image healthBar;

    private float maxHealth;
    private float currentHealth;
    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public void ApplyStats(bool fullHeal)
    {
        maxHealth = stats.GetFinalStat(PlayerStatType.Health);
        if (fullHeal) currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 dir)
    {
        int reduced = Mathf.RoundToInt(amount *
            (1f - stats.GetDamageReductionPercent() / 100f));

        currentHealth -= Mathf.Max(1, reduced);
        UpdateUI();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
    private void UpdateUI()
    {
        if (healthBar)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void ForceRefresh()
    {
        ApplyStats(true);
    }

}
