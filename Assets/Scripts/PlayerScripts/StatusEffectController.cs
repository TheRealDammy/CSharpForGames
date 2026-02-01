using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    private readonly List<StatusEffect> active = new();

    public void AddEffect(StatusEffect effect)
    {
        active.Add(effect);
        effect.Apply(gameObject);
    }
}
