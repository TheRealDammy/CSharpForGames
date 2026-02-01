using UnityEngine;

[CreateAssetMenu(menuName = "Status/Burn")]
public class BurnEffect : StatusEffect
{
    public int damagePerTick;

    public override void Apply(GameObject target)
    {
        // Implementation for applying the burn effect, if needed.
        // Leave empty if no action is required on apply.
    }

    public override void Tick(GameObject target)
    {
        target.GetComponent<IDamageable>()
            ?.TakeDamage(damagePerTick, Vector2.zero, Vector2.zero);
    }

    public override void End(GameObject target)
    {
        // Implementation for ending the burn effect, if needed.
        // Leave empty if no action is required on end.
    }
}
