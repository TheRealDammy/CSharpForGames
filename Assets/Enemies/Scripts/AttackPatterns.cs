using UnityEngine;

public abstract class EnemyAttackPattern : ScriptableObject
{
    public abstract void Execute(EnemyController enemy);
}
