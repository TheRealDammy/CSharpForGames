using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Room lastCheckpointRoom;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCheckpointRoom(Room room)
    {
        lastCheckpointRoom = room;
    }

    public void RespawnPlayer(GameObject player)
    {
        if (lastCheckpointRoom == null) return;

        Vector2 spawnTile =
            lastCheckpointRoom.InnerTiles.Count > 0
                ? (Vector2)lastCheckpointRoom.InnerTiles.First()
                : (Vector2)lastCheckpointRoom.FloorTiles.First();

        player.transform.position =
            spawnTile + Vector2.one * 0.5f;

        ResetRoom(lastCheckpointRoom);
    }

    private void ResetRoom(Room room)
    {
        // 1️⃣ Reset enemies
        foreach (var enemy in room.EnemiesInTheRoom)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        room.EnemiesInTheRoom.Clear();

        // 2️⃣ Clear traps
        foreach (var trap in room.TrapsInRoom)
        {
            if (trap != null)
                Destroy(trap);
        }

        room.TrapsInRoom.Clear();

        // 3️⃣ Respawn enemies using AgentPlacer
        AgentPlacer placer = FindFirstObjectByType<AgentPlacer>();

        if (placer != null)
        {
            placer.RespawnRoom(room);
        }
    }
}
