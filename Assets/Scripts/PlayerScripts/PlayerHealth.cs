using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float iFrames = 0.5f;
    [SerializeField] private float flashTime = 0.06f;

    private float currentHealth;
    private bool invincible;
    private SpriteRenderer sr;
    private Color original;

    [SerializeField] private Image healthBar;
    private ExperienceSystem expSystem;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        expSystem = GetComponent<ExperienceSystem>();
        if (sr) original = sr.color;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (invincible) return;

        currentHealth -= Mathf.Max(1, amount);
        if (sr) StartCoroutine(Flash());
        StartCoroutine(IFrames());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator IFrames()
    {
        invincible = true;
        yield return new WaitForSeconds(iFrames);
        invincible = false;
    }

    private IEnumerator Flash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sr.color = original;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void Die()
    {
        expSystem.ResetExperience();

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("DeathScene");

        DestroyImmediate(gameObject);
    }
}
