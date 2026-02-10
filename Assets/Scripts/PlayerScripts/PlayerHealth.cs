using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Image healthBar;
    [SerializeField] private PlayerSFX sfx;


    private float maxHealth;
    private float currentHealth;
    private PlayerStats stats;
    private ExperienceSystem experienceSystem;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        sfx = GetComponentInChildren<PlayerSFX>();
        experienceSystem = GetComponent<ExperienceSystem>();
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

        sfx?.PlayHit();

        if (currentHealth <= 0)
        {
            Die();
        }        
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

    public void Die()
    {
        experienceSystem?.HandlePlayerDeath();
        SceneManager.LoadSceneAsync("GameOver");
        SceneManager.UnloadSceneAsync("Dungeon");
    }
}
