using UnityEngine;

public class SwordsmanCombatController : CombatController
{
    [Header("Melee")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform attackOrigin;
    

    private int comboStep = 0;
    private float comboResetTime = 0.6f;
    private float comboTimer;
    private int damage = 30;

    protected override void Awake()
    {
        base.Awake();

        if (attackOrigin == null)
        {
            attackOrigin = transform.Find("AttackOrigin");
        }

        if (attackOrigin == null)
        {
            Debug.LogError("AttackOrigin missing on SwordsmanCombat prefab");
        }

        if (hitMask == 0)
        {
            hitMask = LayerMask.GetMask("Enemies", "Props");
            Debug.LogWarning("HitMask auto-assigned");
        }

        baseDamage = damage;
    }  

    protected override void ExecuteAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        comboStep = (comboStep + 1) % 3;
        comboTimer = comboResetTime;

        animator.SetTrigger("Attack");

        DealDamage();
        HitStop();
        CameraShake.Instance.Shake(0.15f, 0.08f);
    }

    private void DealDamage()
    {
        Vector2 dir = movement.GetFacingDirection();

        Vector2 origin = attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Vector2 center = origin + lastDirection * attackRange;

        Collider2D[] all = Physics2D.OverlapCircleAll(center, attackRadius);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius, hitMask);

        int damage = GetFinalDamage();

        Debug.Log($"Dealing {damage} damage to enemies in range.");

        foreach (var hit in hits)
        {
            IDamageable dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Vector2 hitPoint = hit.ClosestPoint(center);
                dmg.TakeDamage(damage, hitPoint, lastDirection);
            }
            if (dmg == null)
            {
                Debug.LogWarning($"Collider {hit.name} does not implement IDamageable.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;

        Gizmos.color = Color.red;
        Vector2 center =
            (Vector2)attackOrigin.position +
            lastDirection * attackRange;

        Gizmos.DrawWireSphere(center, attackRadius);
    }


    protected override void Update()
    {
        base.Update();

        if (comboTimer > 0)
            comboTimer -= Time.deltaTime;
        else
            comboStep = 0;
    }
}
