using UnityEngine;

[CreateAssetMenu(menuName = "Status/Bleed")]
public class BleedEffect : StatusEffect
{
    public int tickDamage;

    public override void Apply(GameObject target)
    {
        // Implement logic to apply bleed effect to the target
    }

    public override void Tick(GameObject target)
    {
        target.GetComponent<IDamageable>()
            ?.TakeDamage(tickDamage, Vector2.zero, Vector2.zero);
    }

    public override void End(GameObject target)
    {
        // Implement logic to end bleed effect on the target
    }
}
