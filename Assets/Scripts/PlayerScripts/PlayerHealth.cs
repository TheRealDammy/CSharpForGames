using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("UI")]
    [SerializeField] private Image healthBar;

    private float maxHealth;
    private float currentHealth;

    private PlayerStats stats;
    private PlayerSFX sfx;
    private ExperienceSystem experienceSystem;

    private bool isDead;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        sfx = GetComponentInChildren<PlayerSFX>();
        experienceSystem = GetComponent<ExperienceSystem>();
    }

    public void ApplyStats(bool fullHeal)
    {
        maxHealth = stats.GetFinalStat(PlayerStatType.Health);

        if (fullHeal || currentHealth <= 0)
            currentHealth = maxHealth;

        UpdateUI();
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 dir)
    {
        if (isDead) return;

        int reduced = Mathf.RoundToInt(
            amount * (1f - stats.GetDamageReductionPercent() / 100f)
        );

        currentHealth -= Mathf.Max(1, reduced);
        UpdateUI();

        sfx?.PlayHit();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        UpdateUI();

        experienceSystem?.HandlePlayerDeath();

        // Disable control
        GetComponent<PlayerInputHandler>().enabled = false;
        GetComponent<TopDownCharacterController>().enabled = false;

        RespawnManager.Instance?.PlayerDied(this);
    }

    public void RespawnAt(Vector3 position)
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateUI();

        // Re-enable control
        GetComponent<PlayerInputHandler>().enabled = true;
        GetComponent<TopDownCharacterController>().enabled = true;
    }

    private void UpdateUI()
    {
        if (healthBar && maxHealth > 0)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void ForceRefresh()
    {
        ApplyStats(true);
    }
    public void RestoreFull()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
}
