using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }

    [Header("Death")]
    [SerializeField] private float destroyDelay = 0.35f; // set to your death anim length

    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    [SerializeField] private Image healthBar;
    [SerializeField] private Canvas healthBarCanvas;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;

        animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
        healthBarCanvas.enabled = false;
    }

    public void Init(int maxHp)
    {
        maxHP = Mathf.Max(1, maxHp);
        currentHP = maxHP;
    }

    public void Update()
    {
        healthBar.fillAmount = (float)currentHP / (float)maxHP;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        animator.SetTrigger("Hurt");

        if (healthBarCanvas != null)
            healthBarCanvas.enabled = true;

        currentHP -= Mathf.Max(1, amount);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // stop movement immediately
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // disable colliders so it doesn't block / get hit multiple times
         foreach (var c in colliders)
           if (c != null) c.enabled = false;

        animator.SetBool("isDead", true);

        // destroy after delay so death anim can show
        Destroy(gameObject,  destroyDelay);
    }
}
