using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack Pattern/Rush")]
public class RushAttackPattern : EnemyAttackPattern
{
    public override void Execute(EnemyController enemy)
    {
        //enemy.RushTowardsPlayer();
    }
}
