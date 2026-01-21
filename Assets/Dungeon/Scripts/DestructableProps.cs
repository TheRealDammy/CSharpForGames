using System.Collections;
using UnityEngine;

public class DestructibleProp : MonoBehaviour, IDamageable
{
    private int hp;
    private Prop propData;
    private Room owningRoom;
    private DungeonData dungeonData;
    private Vector2Int anchorTile;

    [SerializeField] private int TestLookingForImplementation;


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
        for (int i = 0; i < propData.numPickupsToSpawn; i++)
        {
            Debug.Log("Attempting to spawn pickup from destructible prop.");
            if (propData.pickupPrefab != null && Random.value <= propData.pickupSpawnChance)
            {
                int randomPickup = Random.Range(0, propData.pickupPrefab.Length - 1);

                Vector3 spawnPos = (Vector2)transform.position + new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
                Instantiate(propData.pickupPrefab[randomPickup], spawnPos, Quaternion.identity);
                Debug.Log("Spawned pickup from destructible prop.");
            }
        }

        if (owningRoom != null)
        {
            owningRoom.PropObjectReferences.Remove(gameObject);
            owningRoom.PropPositions.Remove(anchorTile); // assumes 1x1 destructibles
        }

        Destroy(gameObject); 
    }
}
