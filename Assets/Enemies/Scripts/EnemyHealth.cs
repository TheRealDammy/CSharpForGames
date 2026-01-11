using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    [Header("Flash")]
    [SerializeField] private float flashTime = 0.06f;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 0.35f; // set to your death anim length
    [SerializeField] private string hurtTrigger = "Hurt";
    [SerializeField] private string deadTrigger = "Dead";

    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private bool isDead;

    private void Awake()
    {
        // Grab all renderers so flash always works even with different prefab layouts
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;

        animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void Init(int maxHp)
    {
        MaxHP = Mathf.Max(1, maxHp);
        CurrentHP = MaxHP;
        isDead = false;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (isDead) return;

        CurrentHP -= Mathf.Max(1, amount);

        // stop only our flash coroutine (not everything on the object)
        StopCoroutine(nameof(Flash));
        StartCoroutine(nameof(Flash));

        // optional hurt animation trigger
        if (animator != null && !string.IsNullOrEmpty(hurtTrigger))
            animator.SetTrigger(hurtTrigger);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private IEnumerator Flash()
    {
        // flash to white
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = Color.white;

        yield return new WaitForSeconds(flashTime);

        // restore
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // stop movement immediately
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // disable colliders so it doesn't block / get hit multiple times
        if (colliders != null)
        {
            foreach (var c in colliders)
                if (c != null) c.enabled = false;
        }

        // play death animation if possible
        if (animator != null && !string.IsNullOrEmpty(deadTrigger))
            animator.SetTrigger(deadTrigger);

        // destroy after delay so death anim can show
        Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }
}
