using UnityEngine;

public abstract class CombatController : MonoBehaviour
{
    protected Animator animator;
    protected PlayerStats stats;
    protected PlayerInputHandler input;
    protected TopDownCharacterController movement;
    protected Transform owner;
    protected Vector2 lastDirection = Vector2.down;

    protected bool isAttacking;
    protected float lastAttackTime;
    protected PlayerSFX sfx;


    [Header("Core Combat")]
    [SerializeField] protected float attackCooldown = 0.3f;
    [SerializeField] protected float hitStopTime = 0.06f;

    // Expose a base damage value that represents the unmodified damage (set in Inspector).
    [SerializeField] protected int baseDamage = 10;

    // Computed final damage after applying stats (not serialized).
    protected int finalDamage;

    protected virtual void Awake()
    {
        owner = transform.parent != null ? transform.parent : transform;

        animator = GetComponentInParent<Animator>();
        stats = GetComponentInParent<PlayerStats>();
        input = GetComponentInParent<PlayerInputHandler>();
        movement = GetComponentInParent<TopDownCharacterController>();
        sfx = GetComponentInParent<PlayerSFX>();

        if (input == null)
            Debug.LogError("[CombatController] Missing PlayerInputHandler");

        if (movement == null)
            Debug.LogError("[CombatController] Missing TopDownCharacterController");
    }

    protected virtual void Update()
    {
        //if (input == null || movement == null)
            //return;

        //if (input.AttackPressed)
            //TryAttack();

        lastDirection = movement.GetFacingDirection();
    }

    public virtual void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        sfx?.PlayAttack();
        ExecuteAttack();
        RunStats.Instance?.RegisterDamage(GetFinalDamage());
    }

    // Apply stats to compute final damage.
    public void ApplyStats()
    {
        if (stats == null)
        {
            Debug.LogError("[CombatController] Cannot ApplyStats: missing PlayerStats");
            return;
        }

        int strength = stats.GetStatLevel(PlayerStatType.Strength);

        // scaling: +6 per strength point
        float multiplier = 1f + 0.2f * strength;

        // Do not mutate baseDamage; compute and store finalDamage.
        finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        Debug.Log($"[CombatController] Applied Stats: Strength={strength}, BaseDamage={baseDamage}, FinalDamage={finalDamage}");
    }

    public int GetFinalDamage()
    {
        if (stats == null)
        {
            Debug.LogError("CombatController missing PlayerStats");
            // Fallback to baseDamage if stats are missing.
            return baseDamage;
        }

        // If ApplyStats has already computed finalDamage, return it.
        if (finalDamage > 0)
            return finalDamage;

        // Otherwise compute on the fly to ensure a sensible value.
        int strength = stats.GetStatLevel(PlayerStatType.Strength);
        float multiplier = 1f + 0.05f * strength;
        return Mathf.RoundToInt(baseDamage * multiplier);

    }

    protected abstract void ExecuteAttack();

    protected void HitStop()
    {
        StartCoroutine(HitStopRoutine());
    }

    private System.Collections.IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopTime);
        Time.timeScale = 1f;
    }
}
