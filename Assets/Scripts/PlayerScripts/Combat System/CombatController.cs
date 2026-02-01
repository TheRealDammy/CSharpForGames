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

    [Header("Core Combat")]
    [SerializeField] protected float attackCooldown = 0.3f;
    [SerializeField] protected float hitStopTime = 0.06f;
    protected int baseDamage;

    protected virtual void Awake()
    {
        owner = transform;
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        input = GetComponent<PlayerInputHandler>();
        movement = GetComponent<TopDownCharacterController>();
    }

    protected virtual void Update()
    {
        if (input.AttackPressed)
            TryAttack();
        lastDirection = movement.GetFacingDirection();
    }

    public virtual void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        ExecuteAttack();
    }

    protected abstract void ExecuteAttack();

    protected void HitStop()
    {
        StartCoroutine(HitStopRoutine());
    }
    public void SetBaseDamage(int value)
    {
        baseDamage = value;
    }

    protected int GetFinalDamage()
    {
        if (stats == null)
        {
            Debug.LogError("CombatController missing PlayerStats");
            return baseDamage;
        }

        int strength = stats.GetFinalStat(PlayerStatType.Strength);

        // scaling: +20% per strength point
        float multiplier = 1f + (strength * 0.2f);

        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    public int GetDamage()
    {
        return GetFinalDamage();
    }

    private System.Collections.IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopTime);
        Time.timeScale = 1f;
    }
}
