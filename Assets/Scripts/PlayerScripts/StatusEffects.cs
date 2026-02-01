using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public float duration;

    public abstract void Apply(GameObject target);
    public abstract void Tick(GameObject target);
    public abstract void End(GameObject target);
}
