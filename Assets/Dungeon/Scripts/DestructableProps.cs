using System.Collections;
using UnityEngine;

public class DestructibleProp : MonoBehaviour, IDamageable
{
    private int hp;
    private Prop propData;
    private Room owningRoom;
    private DungeonData dungeonData;
    private Vector2Int anchorTile;

    public void Init(Prop data, Room room, DungeonData dungeon, Vector2Int placedTile)
    {
        propData = data;
        owningRoom = room;
        dungeonData = dungeon;
        anchorTile = placedTile;
        hp = propData.MaxHP;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        hp -= Mathf.Max(1, amount);
        if (hp <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        Vector2 explosionPosition = anchorTile;

        VFXManager.CreateExplosion(explosionPosition);

        if (owningRoom != null)
        {
            owningRoom.PropObjectReferences.Remove(gameObject);
            owningRoom.PropPositions.Remove(anchorTile); // assumes 1x1 destructibles
        }

        Destroy(gameObject);  
    }
}
