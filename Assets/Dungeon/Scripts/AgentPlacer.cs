using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentPlacer : MonoBehaviour
{
    [Header("Player Placement Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int playerRoomIndex = 0;

    [Header("Enemy Placement Settings")]
    [Tooltip("If provided per room index, overrides random range below.")]
    [SerializeField] private List<int> roomEnemiesCount = new List<int>();

    [SerializeField] private List<EnemyTypeSO> enemyTypes = new List<EnemyTypeSO>();
    [SerializeField] private GameObject enemyPrefab;

    [Header("Distribution")]
    [Tooltip("Minimum Manhattan distance between chosen spawn tiles (relaxes if needed).")]
    [SerializeField] private int minSpacing = 3;

    [Tooltip("0=random, 1=prefer inner tiles")]
    [SerializeField, Range(0f, 1f)] private float centerBias = 0.7f;

    [Tooltip("0=prefer inner, 1=prefer corridor tiles")]
    [SerializeField, Range(0f, 1f)] private float corridorBias = 0.35f;

    [Header("Fallback if roomEnemiesCount missing/0")]
    [SerializeField] private Vector2Int enemiesPerRoomRange = new Vector2Int(1, 4);
    [SerializeField] private bool guaranteeAtLeastOneEnemyPerRoom = true;

    [Header("Room difficulty")]
    [Tooltip("How much difficulty increases per room index (0.12 = +12% per room).")]
    [SerializeField, Range(0f, 0.5f)] private float difficultyPerRoom = 0.12f;

    [Tooltip("Extra multiplier for rooms far from the player room.")]
    [SerializeField, Range(0f, 1f)] private float distanceFromPlayerRoomBonus = 0.25f;

    [Header("Elite packs")]
    [SerializeField, Range(0f, 1f)] private float elitePackChanceBase = 0.10f;
    [SerializeField, Range(0f, 1f)] private float elitePackChanceMax = 0.35f;
    [SerializeField] private Vector2Int elitePackSize = new Vector2Int(3, 6);

    [Header("Groups")]
    [Tooltip("Tiles around the anchor to spawn group/pack members.")]
    [SerializeField, Range(0.5f, 6f)] private float groupRadius = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = false;
    [SerializeField] private bool verboseLogs = false;

    private DungeonData dungeonData;

    private static readonly Vector2Int[] dirs =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    public void Init(DungeonData data) => dungeonData = data;

    public void PlaceAgents()
    {
        // Defensive: make sure data exists
        if (dungeonData == null || dungeonData.rooms == null || dungeonData.rooms.Count == 0)
        {
            Debug.LogError("AgentPlacer: dungeonData/rooms not ready", this);
            return;
        }
        if (dungeonData.path == null)
        {
            Debug.LogError("AgentPlacer: dungeonData.path is null (corridors not assigned).", this);
            return;
        }
        if (enemyPrefab == null)
        {
            Debug.LogError("AgentPlacer: enemyPrefab not assigned", this);
            return;
        }

        // Sanitize enemyTypes (null entries cause weird picks)
        enemyTypes = enemyTypes.Where(e => e != null).ToList();
        if (enemyTypes.Count == 0)
        {
            Debug.LogError("AgentPlacer: enemyTypes empty", this);
            return;
        }

        if (verboseLogs)
        {
            Debug.Log("EnemyTypes in list: " + string.Join(", ",
                enemyTypes.Select(e => e ? $"{e.name} (w={e.spawnWeight})" : "NULL")));
        }

        SpawnPlayer();

        // Spawn enemies per room
        for (int i = 0; i < dungeonData.rooms.Count; i++)
        {
            Room room = dungeonData.rooms[i];

            // Build accessible tiles using BFS from entry
            if (!TryBuildAccessibleTiles(room))
            {
                // If BFS couldn't run (no entry tile), fall back to room floor tiles
                // so rooms still get enemies (especially with blobs).
                room.PositionsAccessibleFromPath = room.FloorTiles
                    .Where(t => !room.PropPositions.Contains(t))
                    .OrderBy(_ => Guid.NewGuid())
                    .ToList();
            }

            int countForRoom = GetEnemyCountForRoom(i);

            // Skip player room
            if (i == playerRoomIndex) countForRoom = 0;

            if (countForRoom > 0)
            {
                float difficulty = ComputeRoomDifficulty(i);
                PlaceEnemiesSmart(room, countForRoom, difficulty);
            }
        }
    }

    private bool TryBuildAccessibleTiles(Room room)
    {
        if (room == null || room.FloorTiles == null || room.FloorTiles.Count == 0)
            return false;

        // Find entry tile adjacent to corridor
        if (!TryGetRoomEntry(room, out var entryTile))
            return false;

        // BFS within room from entry, avoiding props
        var graph = new RoomGraph(room.FloorTiles);
        Dictionary<Vector2Int, int> visited = graph.RunBFS(entryTile, room.PropPositions);

        room.PositionsAccessibleFromPath = visited.Keys
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        return room.PositionsAccessibleFromPath.Count > 0;
    }

    private int GetEnemyCountForRoom(int roomIndex)
    {
        int count = 0;

        if (roomEnemiesCount != null && roomEnemiesCount.Count > roomIndex)
            count = roomEnemiesCount[roomIndex];

        if (count <= 0 && guaranteeAtLeastOneEnemyPerRoom)
            count = UnityEngine.Random.Range(enemiesPerRoomRange.x, enemiesPerRoomRange.y + 1);

        return Mathf.Max(0, count);
    }

    private float ComputeRoomDifficulty(int roomIndex)
    {
        float baseDiff = 1f + roomIndex * difficultyPerRoom;

        int distRooms = Mathf.Abs(roomIndex - playerRoomIndex);
        float distBonus = 1f + distRooms * distanceFromPlayerRoomBonus * 0.1f;

        return Mathf.Clamp(baseDiff * distBonus, 1f, 3f);
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        if (playerRoomIndex < 0 || playerRoomIndex >= dungeonData.rooms.Count) return;

        Room playerRoom = dungeonData.rooms[playerRoomIndex];

        Vector2Int spawnTile =
            (playerRoom.InnerTiles != null && playerRoom.InnerTiles.Count > 0)
                ? playerRoom.InnerTiles.OrderBy(_ => Guid.NewGuid()).First()
                : playerRoom.FloorTiles.OrderBy(_ => Guid.NewGuid()).First();

        GameObject player = Instantiate(playerPrefab);
        player.transform.position = (Vector2)spawnTile + Vector2.one * 0.5f;
        dungeonData.PlayerReference = player;
    }

    private bool TryGetRoomEntry(Room room, out Vector2Int entry)
    {
        // A room tile is "entry" if it's inside room and adjacent to a corridor tile.
        foreach (var tile in room.FloorTiles)
        {
            foreach (var d in dirs)
            {
                if (dungeonData.path.Contains(tile + d))
                {
                    entry = tile;
                    return true;
                }
            }
        }

        entry = default;
        return false;
    }

    // ---------------------------
    // SMART SPAWNING
    // ---------------------------

    private void PlaceEnemiesSmart(Room room, int enemyCount, float difficulty)
    {
        if (dungeonData.PlayerReference == null) return;
        if (room.PositionsAccessibleFromPath == null || room.PositionsAccessibleFromPath.Count == 0) return;

        // Candidate tiles: allow corridors too, only exclude prop tiles
        var candidates = room.PositionsAccessibleFromPath
            .Where(t => !room.PropPositions.Contains(t))
            .Distinct()
            .ToList();

        if (candidates.Count == 0) return;

        HashSet<Vector2Int> inner = room.InnerTiles ?? new HashSet<Vector2Int>();

        // Score tiles: randomness + inner bias + corridor bias
        candidates = candidates.OrderByDescending(t =>
        {
            float rand = UnityEngine.Random.value;

            float innerScore = inner.Contains(t) ? 1f : 0f;
            float pathScore = dungeonData.path.Contains(t) ? 1f : 0f;

            // mix inner vs corridor
            float biased = Mathf.Lerp(innerScore, pathScore, corridorBias);

            // then blend with random
            float mix = Mathf.Lerp(rand, rand + biased, 0.6f);

            // apply centerBias to slightly favor inner tiles overall
            return Mathf.Lerp(mix, mix + innerScore, centerBias * 0.35f);
        }).ToList();

        // Pick spread tiles (farthest-point-ish)
        List<Vector2Int> chosen = PickSpreadTiles(candidates, enemyCount, minSpacing);

        // If too few, fill remaining (no spacing)
        if (chosen.Count < enemyCount)
            chosen.AddRange(candidates.Except(chosen).Take(enemyCount - chosen.Count));

        float eliteChance = Mathf.Lerp(elitePackChanceBase, elitePackChanceMax,
            Mathf.InverseLerp(1f, 3f, difficulty));

        // anti-streak to stop "only slimes"
        EnemyTypeSO lastType = null;

        int idx = 0;
        while (idx < chosen.Count)
        {
            int remaining = chosen.Count - idx;

            // Elite pack
            if (remaining >= 3 && UnityEngine.Random.value < eliteChance)
            {
                int packTotal = UnityEngine.Random.Range(elitePackSize.x, elitePackSize.y + 1);
                packTotal = Mathf.Clamp(packTotal, 3, remaining);

                int spawned = SpawnElitePack(room, chosen[idx], packTotal, candidates, difficulty, ref lastType);
                idx += Mathf.Max(1, spawned);
                continue;
            }

            // Group behavior
            EnemyTypeSO groupType = PickTypeWithAntiStreak(lastType);
            if (groupType != null && groupType.prefersGroups &&
                UnityEngine.Random.value < groupType.groupChance && remaining >= 2)
            {
                int groupSize = UnityEngine.Random.Range(groupType.groupMin, groupType.groupMax + 1);
                groupSize = Mathf.Clamp(groupSize, 2, remaining);

                int spawned = SpawnGroup(room, chosen[idx], groupType, groupSize, candidates, difficulty);
                lastType = groupType;
                idx += Mathf.Max(1, spawned);
                continue;
            }

            // Single
            SpawnSingle(room, chosen[idx], difficulty, ref lastType);
            idx++;
        }
    }

    private EnemyTypeSO PickTypeWithAntiStreak(EnemyTypeSO lastType)
    {
        return WeightedRandoms.Pick(enemyTypes, t =>
        {
            if (t == null) return 0f;
            float w = Mathf.Max(0f, t.spawnWeight);
            if (lastType != null && t == lastType) w *= 0.55f; // anti-streak
            return w;
        });
    }

    private void SpawnSingle(Room room, Vector2Int tile, float difficulty, ref EnemyTypeSO lastType)
    {
        EnemyTypeSO type = PickTypeWithAntiStreak(lastType);
        if (type == null) return;

        int vIndex = PickVariantIndex(type, difficulty);
        EnemyVariant variant = (EnemyVariant)vIndex;

        if (verboseLogs) Debug.Log($"SpawnSingle -> {type.name} v{(int)variant} @ {tile}");
        SpawnEnemy(room, tile, type, variant);

        lastType = type;
    }

    private int SpawnGroup(Room room, Vector2Int anchor, EnemyTypeSO type, int groupSize, List<Vector2Int> candidates, float difficulty)
    {
        var local = candidates
            .Where(t => !room.PropPositions.Contains(t))
            .Where(t => Manhattan(t, anchor) <= Mathf.CeilToInt(groupRadius))
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        if (local.Count == 0) local.Add(anchor);

        int spawned = 0;
        for (int i = 0; i < groupSize && local.Count > 0; i++)
        {
            var tile = local[0];
            local.RemoveAt(0);

            int vIndex = PickVariantIndex(type, difficulty * 0.9f);
            if (verboseLogs) Debug.Log($"SpawnGroup -> {type.name} v{vIndex} @ {tile}");

            SpawnEnemy(room, tile, type, (EnemyVariant)vIndex);
            spawned++;
        }

        return spawned;
    }

    private int SpawnElitePack(Room room, Vector2Int anchor, int total, List<Vector2Int> candidates, float difficulty, ref EnemyTypeSO lastType)
    {
        EnemyTypeSO type = PickTypeWithAntiStreak(lastType);
        if (type == null) return 0;

        var local = candidates
            .Where(t => !room.PropPositions.Contains(t))
            .Where(t => Manhattan(t, anchor) <= Mathf.CeilToInt(groupRadius + 1))
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        if (local.Count == 0) local.Add(anchor);

        int spawned = 0;

        // Elite leader (prefer elite variant if it has weight)
        if (local.Count > 0)
        {
            int eliteVariant = PickEliteVariantIndex(type);
            if (verboseLogs) Debug.Log($"SpawnEliteLeader -> {type.name} v{eliteVariant} @ {local[0]}");
            SpawnEnemy(room, local[0], type, (EnemyVariant)eliteVariant);
            local.RemoveAt(0);
            spawned++;
        }

        // Minions
        for (int i = 1; i < total && local.Count > 0; i++)
        {
            var tile = local[0];
            local.RemoveAt(0);

            int vIndex = PickVariantIndex(type, difficulty * 0.8f);
            if (verboseLogs) Debug.Log($"SpawnEliteMinion -> {type.name} v{vIndex} @ {tile}");

            SpawnEnemy(room, tile, type, (EnemyVariant)vIndex);
            spawned++;
        }

        lastType = type;
        return spawned;
    }

    private void SpawnEnemy(Room room, Vector2Int tile, EnemyTypeSO type, EnemyVariant variant)
    {
        if (type == null) return;
        if (dungeonData.PlayerReference == null) return;

        // If another enemy/prop already reserved this tile, skip
        if (room.PropPositions.Contains(tile)) return;

        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = (Vector2)tile + Vector2.one * 0.5f;

        var controller = enemy.GetComponent<EnemyController>();
        if (controller == null)
        {
            Debug.LogError("Enemy prefab missing EnemyController", enemy);
            Destroy(enemy);
            return;
        }

        // IMPORTANT: EnemyController.Init must return bool
        bool ok = controller.Init(type, variant, dungeonData.PlayerReference.transform);
        if (!ok)
        {
            // Prevent "ghost enemies"
            Destroy(enemy);
            return;
        }

        room.EnemiesInTheRoom.Add(enemy);
        room.PropPositions.Add(tile); // reserve tile so nothing overlaps
    }

    // ---------------------------
    // VARIANT WEIGHTS + DIFFICULTY
    // ---------------------------

    // Uses per-variant spawnWeight (type.variants[i].spawnWeight), then biases by difficulty.
    private int PickVariantIndex(EnemyTypeSO type, float difficulty)
    {
        if (type == null || type.variants == null || type.variants.Length < 3)
            return UnityEngine.Random.Range(0, 3);

        float w0 = Mathf.Max(0f, type.variants[0] != null ? type.variants[0].spawnWeight : 0f);
        float w1 = Mathf.Max(0f, type.variants[1] != null ? type.variants[1].spawnWeight : 0f);
        float w2 = Mathf.Max(0f, type.variants[2] != null ? type.variants[2].spawnWeight : 0f);

        // If all are 0, fallback random
        if (w0 + w1 + w2 <= 0f)
            return UnityEngine.Random.Range(0, 3);

        // Difficulty pushes stronger variants later
        float t = Mathf.InverseLerp(1f, 3f, difficulty); // 0..1
        w0 *= Mathf.Lerp(1f, 0.55f, t);
        w1 *= Mathf.Lerp(1f, 1.25f, t);
        w2 *= Mathf.Lerp(1f, 1.90f, t);

        float total = w0 + w1 + w2;
        float r = UnityEngine.Random.value * total;

        if ((r -= w0) <= 0f) return 0;
        if ((r -= w1) <= 0f) return 1;
        return 2;
    }

    private int PickEliteVariantIndex(EnemyTypeSO type)
    {
        if (type == null || type.variants == null || type.variants.Length < 3)
            return 2;

        float eliteW = Mathf.Max(0f, type.variants[2] != null ? type.variants[2].spawnWeight : 0f);
        float strongW = Mathf.Max(0f, type.variants[1] != null ? type.variants[1].spawnWeight : 0f);

        if (eliteW > 0f) return 2;
        if (strongW > 0f) return 1;
        return 0;
    }

    // Farthest-point-ish spread picker with minimum spacing
    private List<Vector2Int> PickSpreadTiles(List<Vector2Int> candidates, int count, int spacing)
    {
        var chosen = new List<Vector2Int>();
        if (candidates == null || candidates.Count == 0 || count <= 0) return chosen;

        chosen.Add(candidates[UnityEngine.Random.Range(0, candidates.Count)]);

        while (chosen.Count < count)
        {
            Vector2Int best = default;
            int bestDist = -1;
            bool found = false;

            foreach (var c in candidates)
            {
                // skip already chosen
                bool already = false;
                for (int i = 0; i < chosen.Count; i++)
                {
                    if (chosen[i] == c) { already = true; break; }
                }
                if (already) continue;

                // ensure spacing from all chosen
                int minD = int.MaxValue;
                for (int i = 0; i < chosen.Count; i++)
                    minD = Mathf.Min(minD, Manhattan(c, chosen[i]));

                if (minD < spacing) continue;

                if (minD > bestDist)
                {
                    bestDist = minD;
                    best = c;
                    found = true;
                }
            }

            if (!found) break;
            chosen.Add(best);
        }

        return chosen;
    }

    private int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (dungeonData == null || !showGizmo) return;

        foreach (Room room in dungeonData.rooms)
        {
            Color color = Color.green; color.a = 0.3f;
            Gizmos.color = color;

            if (room.PositionsAccessibleFromPath == null) continue;
            foreach (Vector2Int pos in room.PositionsAccessibleFromPath)
                Gizmos.DrawCube((Vector2)pos + Vector2.one * 0.5f, Vector2.one);
        }
    }
#endif
}

// -------------------------------------------------------
// WeightedRandom helper (fixes your "where is it from?")
// -------------------------------------------------------
public static class WeightedRandoms
{
    public static T Pick<T>(IList<T> items, Func<T, float> weight)
    {
        if (items == null || items.Count == 0) return default;

        float total = 0f;
        for (int i = 0; i < items.Count; i++)
            total += Mathf.Max(0f, weight(items[i]));

        if (total <= 0f)
            return items[UnityEngine.Random.Range(0, items.Count)];

        float r = UnityEngine.Random.value * total;
        for (int i = 0; i < items.Count; i++)
        {
            r -= Mathf.Max(0f, weight(items[i]));
            if (r <= 0f) return items[i];
        }

        return items[items.Count - 1];
    }
}

// -------------------------------------------------------
// RoomGraph (BFS) - returns reachable room tiles
// -------------------------------------------------------
public class RoomGraph
{
    private readonly HashSet<Vector2Int> floor;

    public RoomGraph(HashSet<Vector2Int> floorTiles)
    {
        floor = floorTiles ?? new HashSet<Vector2Int>();
    }

    public Dictionary<Vector2Int, int> RunBFS(Vector2Int entry, HashSet<Vector2Int> occupied)
    {
        occupied ??= new HashSet<Vector2Int>();

        var visited = new Dictionary<Vector2Int, int>();
        var q = new Queue<(Vector2Int pos, int dist)>();

        if (!floor.Contains(entry) || occupied.Contains(entry))
            return visited;

        q.Enqueue((entry, 0));
        visited[entry] = 0;

        while (q.Count > 0)
        {
            var (cur, dist) = q.Dequeue();

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left })
            {
                var next = cur + dir;
                if (!floor.Contains(next)) continue;
                if (occupied.Contains(next)) continue;
                if (visited.ContainsKey(next)) continue;

                visited[next] = dist + 1;
                q.Enqueue((next, dist + 1));
            }
        }

        return visited;
    }
}
