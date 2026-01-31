using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Image healthBar;
    [SerializeField] private float iFrames = 0.5f;

    private float maxHealth;
    private float currentHealth;
    private bool invincible;

    private void Start()
    {
        ApplyStats(fullHeal: true);

        PlayerStats.Instance.OnStatChanged += _ => ApplyStats(false);
        PlayerStats.Instance.OnStatsLoaded += () => ApplyStats(true);
    }

    public void ApplyStats(bool fullHeal)
    {
        int hpLevel = PlayerStats.Instance.GetStatLevel(PlayerStatType.Health);
        maxHealth = 100 + hpLevel * 20;

        if (fullHeal)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (invincible) return;

        int reduced = ApplyDamageReduction(amount);
        currentHealth -= Mathf.Max(1, reduced);

        StartCoroutine(IFrames());

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    private int ApplyDamageReduction(int incoming)
    {
        int dur = PlayerStats.Instance.GetStatLevel(PlayerStatType.Durability);
        float reduction = dur * 0.04f; // 4% per level
        return Mathf.RoundToInt(incoming * (1f - reduction));
    }

    private IEnumerator IFrames()
    {
        invincible = true;
        yield return new WaitForSeconds(iFrames);
        invincible = false;
    }

    private void Update()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void Die()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DeathScene");
    }
}
