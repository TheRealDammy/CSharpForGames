using UnityEngine;

public class MageCombatController : CombatController
{
    [SerializeField] GameObject spellPrefab;
    [SerializeField] private Transform castPoint;
    [SerializeField] float castRange = 3f;
    [SerializeField] float range = 3.2f;
    [SerializeField] float radius = 0.2f;
    [SerializeField] LayerMask enemyLayers;

    protected override void ExecuteAttack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Cast");

        Vector2 target = (Vector2)transform.position +
            movement.GetFacingDirection() * castRange;

        Instantiate(spellPrefab, target, Quaternion.identity);

        DealDamage();
        HitStop();
        CameraShake.Instance.Shake(0.25f, 0.12f);
    }

    private void DealDamage()
    {
        Vector2 dir = movement.GetFacingDirection();
        Vector2 center = (Vector2)castPoint.position + dir * range;

        foreach (var hit in Physics2D.OverlapCircleAll(center, radius, enemyLayers))
        {
            if (hit.TryGetComponent(out IDamageable dmg))
                dmg.TakeDamage(stats.GetStatLevel(PlayerStatType.Strength) * 2 + 5, center, dir);
        }
    }
}
