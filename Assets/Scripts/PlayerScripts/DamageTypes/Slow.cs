using UnityEngine;

[CreateAssetMenu(menuName = "Status/Slow")]
public class SlowEffect : StatusEffect
{
    public float slowMultiplier = 0.6f;

    public override void Apply(GameObject target)
    {
        target.GetComponent<TopDownCharacterController>()
            ?.ModifySpeed(slowMultiplier);
    }

    public override void Tick(GameObject target)
    {
        // No additional logic needed for Tick in SlowEffect.
        // This method is required to implement the abstract member.
    }

    public override void End(GameObject target)
    {
        target.GetComponent<TopDownCharacterController>()
            ?.ResetSpeed();
    }
}
