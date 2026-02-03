using UnityEngine;

public class ArcherCombatController : CombatController
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float arrowSpeed = 12f;
    [SerializeField] float range = 3.2f;
    [SerializeField] float radius = 0.2f;
    [SerializeField] LayerMask hitMask;
    private int damage;

    protected override void Awake()
    {
        base.Awake();
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
        }
        if (firePoint == null)
        {
            Debug.LogError("FirePoint missing on ArcherCombat prefab");
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
        animator.SetTrigger("Shoot");

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        arrow.GetComponent<Rigidbody2D>().linearVelocity =
            movement.GetFacingDirection() * arrowSpeed;

        DealDamage();
        HitStop();
        CameraShake.Instance.Shake(0.25f, 0.12f);
    }
    private void DealDamage()
    {
        Vector2 dir = movement.GetFacingDirection();
        Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 center = origin + lastDirection * range;

        Collider2D[] all = Physics2D.OverlapCircleAll(center, radius);

        foreach (var c in all)
        {
            Debug.Log($"Found collider: {c.name} | Layer: {LayerMask.LayerToName(c.gameObject.layer)}");
        }


        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, hitMask);
        Debug.Log($"Hits detected: {hits.Length}");
        Debug.Log($"HitMask value: {hitMask.value}");

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
}
