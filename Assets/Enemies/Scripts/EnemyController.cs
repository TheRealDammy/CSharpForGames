using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private bool isInitialized;

    private Rigidbody2D rb;
    private EnemyHealth health;
    private SpriteRenderer sr;
    private Animator animator;

    private EnemyTypeSO type;
    private EnemyVariant variant;
    public EnemyVariantData variantData
    {
        get
        {
            if (type == null) return null;
            int vi = (int)variant;
            if (vi < 0 || vi >= type.variants.Length) return null;
            return type.variants[vi];
        }
    }

    private float lastAttackTime = -999f;

    private float moveSpeed;
    private float aggroRange;
    private float attackRange;
    private float attackCooldown;
    private int damage;

    private Vector2 facingDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        animator = GetComponentInChildren<Animator>(true);

        // Top-down defaults
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Important: keep Dynamic so MovePosition works properly
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reduce "player can push enemies"
        rb.mass = 100f;
        rb.linearDamping = 8f;
        rb.angularDamping = 999f;

        if (health == null) Debug.LogError("EnemyController: EnemyHealth missing.", this);
        if (sr == null) Debug.LogError("EnemyController: SpriteRenderer missing in children.", this);
        // animator can be null if you don't use animations yet (that's fine)
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (animator != null)
        {
            animator.SetFloat("Horizontal", facingDir.x);
            animator.SetFloat("Vertical", facingDir.y);

        }
    }

    /// <summary>
    /// Must return true if fully initialized, false if something is missing.
    /// Spawner will destroy the enemy when false to avoid ghost enemies.
    /// </summary>
    public bool Init(EnemyTypeSO enemyType, EnemyVariant enemyVariant, Transform player)
    {
        if (enemyType == null)
        {
            Debug.LogError("EnemyController.Init: enemyType is null", this);
            return false;
        }

        if (player == null)
        {
            Debug.LogError($"EnemyController.Init: player is null for {enemyType.name}", this);
            return false;
        }

        if (health == null)
        {
            Debug.LogError($"EnemyController.Init: EnemyHealth missing on prefab for {enemyType.name}", this);
            return false;
        }

        if (enemyType.variants == null || enemyType.variants.Length < 3)
        {
            Debug.LogError($"{enemyType.name}: variants must have 3 entries.", this);
            return false;
        }

        int vi = (int)enemyVariant;
        if (vi < 0 || vi >= enemyType.variants.Length || enemyType.variants[vi] == null)
        {
            Debug.LogError($"{enemyType.name}: variant index {vi} is missing/null.", this);
            return false;
        }

        var v = enemyType.variants[vi];

        type = enemyType;
        variant = enemyVariant;
        target = player;

        // Stats
        moveSpeed = Mathf.Max(0.01f, type.moveSpeed * v.speedMultiplier);
        aggroRange = Mathf.Max(0f, type.aggroRange);
        attackRange = Mathf.Max(0.05f, type.attackRange);
        attackCooldown = Mathf.Max(0.05f, type.attackCooldown);
        damage = Mathf.Max(1, Mathf.RoundToInt(type.baseDamage * v.damageMultiplier));

        int maxHP = Mathf.Max(1, Mathf.RoundToInt(type.baseHP * v.hpMultiplier));
        health.Init(maxHP);

        // Visuals: sprite
        if (sr != null)
        {
            sr.sprite = v.spriteOverride != null ? v.spriteOverride : type.defaultSprite;
        }

        // Visuals: animator override per variant
        if (animator != null && v.animatorOverride != null)
        {
            animator.runtimeAnimatorController = v.animatorOverride;
        }

        // Scale
        float s = Mathf.Max(0.1f, v.scaleMultiplier);
        transform.localScale = new Vector3(s, s, 1f);

        // Mark initialized
        isInitialized = true;

        // Reset motion state so it doesn’t “stick”
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.WakeUp();
        }

        return true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || target == null || type == null)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (target.position - transform.position);
        float dist = toPlayer.magnitude;

        // Outside aggro: stop
        if (dist > aggroRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Facing always updates when aggroed
        if (toPlayer.sqrMagnitude > 0.0001f)
            facingDir = toPlayer.normalized;

        // In attack range
        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack();
            return;
        }

        // Chase
        Vector2 step = facingDir * moveSpeed * Time.fixedDeltaTime;
        animator.SetFloat("Speed", facingDir.magnitude);
        rb.MovePosition(rb.position + step);
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        Vector2 center = (Vector2)transform.position + facingDir * 0.6f;
        float radius = 0.45f;

        Collider2D hit = Physics2D.OverlapCircle(center, radius, hitLayers);
        if (hit == null) return;

        if (animator != null)
            animator.SetTrigger("Attack");

        var dmg = hit.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage, hit.ClosestPoint(center), facingDir);
        }
    }
}
