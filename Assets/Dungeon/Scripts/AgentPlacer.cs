using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField] private int minSpacing = 4;

    [Tooltip("0=prefer room interior, 1=prefer near walls (keep this low).")]
    [SerializeField, Range(0f, 1f)] private float edgeBias = 0.15f;

    [Header("Corridor spawns")]
    [SerializeField, Range(0f, 1f)] private float corridorSpawnShare = 0.35f; // share of extra spawns in corridors
    [SerializeField] private int corridorMinSpacing = 5;
    [SerializeField] private bool allowCorridorSpawns = true;

    [Header("Fallback if roomEnemiesCount missing/0")]
    [SerializeField] private Vector2Int enemiesPerRoomRange = new Vector2Int(2, 6);
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

    // Global reserved positions (rooms + corridors) to prevent overlaps.
    private readonly HashSet<Vector2Int> reserved = new HashSet<Vector2Int>();

    private static readonly Vector2Int[] dirs =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    public void Init(DungeonData data) => dungeonData = data;

    public void PlaceAgents()
    {
        reserved.Clear();

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

        // Clean enemy list
        enemyTypes = enemyTypes.Where(e => e != null).ToList();
        if (enemyTypes.Count == 0)
        {
            Debug.LogError("AgentPlacer: enemyTypes empty", this);
            return;
        }

        if (verboseLogs)
        {
            Debug.Log("EnemyTypes: " + string.Join(", ", enemyTypes.Select(e => $"{e.name}(w={e.spawnWeight})")));
        }

        // Pre-reserve all prop tiles (so enemies never overlap props).
        CacheReservedFromProps();

        // Spawn player first
        SpawnPlayer();
        if (dungeonData.PlayerReference == null)
        {
            Debug.LogError("AgentPlacer: PlayerReference not set after SpawnPlayer()", this);
            return;
        }

        // Build counts first (so corridor spawns can be proportional)
        List<int> countsPerRoom = new List<int>();
        int totalRoomEnemies = 0;

        for (int i = 0; i < dungeonData.rooms.Count; i++)
        {
            int c = GetEnemyCountForRoom(i);
            if (i == playerRoomIndex) c = 0;
            countsPerRoom.Add(Mathf.Max(0, c));
            totalRoomEnemies += Mathf.Max(0, c);
        }

        // Spawn per room
        for (int i = 0; i < dungeonData.rooms.Count; i++)
        {
            Room room = dungeonData.rooms[i];
            int countForRoom = countsPerRoom[i];
            if (countForRoom <= 0) continue;

            // Build accessible tiles using BFS; if fails, use all floor tiles.
            EnsureRoomAccessibleTiles(room);

            float difficulty = ComputeRoomDifficulty(i);
            PlaceEnemiesSmart(room, countForRoom, difficulty);
        }

        // Corridor spawns as "extra action" (doesn't steal from rooms)
        if (allowCorridorSpawns && corridorSpawnShare > 0f)
        {
            int corridorCount = Mathf.RoundToInt(totalRoomEnemies * corridorSpawnShare);
            SpawnCorridorEnemies(corridorCount);
        }
    }

    private void CacheReservedFromProps()
    {
        reserved.Clear();
        foreach (var r in dungeonData.rooms)
        {
            if (r?.PropPositions == null) continue;
            foreach (var p in r.PropPositions)
                reserved.Add(p);
        }
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
        if (playerPrefab == null)
        {
            Debug.LogError("AgentPlacer: playerPrefab missing", this);
            return;
        }

        if (playerRoomIndex < 0 || playerRoomIndex >= dungeonData.rooms.Count)
        {
            Debug.LogError("AgentPlacer: invalid playerRoomIndex", this);
            return;
        }

        Room playerRoom = dungeonData.rooms[playerRoomIndex];
        if (playerRoom == null || playerRoom.FloorTiles == null || playerRoom.FloorTiles.Count == 0)
        {
            Debug.LogError("AgentPlacer: player room invalid", this);
            return;
        }

        Vector2Int spawnTile =
            (playerRoom.InnerTiles != null && playerRoom.InnerTiles.Count > 0)
                ? playerRoom.InnerTiles.OrderBy(_ => Guid.NewGuid()).First()
                : playerRoom.FloorTiles.OrderBy(_ => Guid.NewGuid()).First();

        GameObject player = Instantiate(playerPrefab);
        player.transform.position = (Vector2)spawnTile + Vector2.one * 0.5f;
        
        player.name = "Player";
        player.tag = "Player";

        dungeonData.PlayerReference = player;

        // Reserve player tile
        reserved.Add(spawnTile);

        var classManager = PlayerClassHandler.Instance;

        if (classManager == null || classManager.SelectedClass == null)
        {
            Debug.LogError("PlayerClassManager or SelectedClass missing!");
            return;
        }

        var classData = classManager.SelectedClass;

        // Attach combat
        CombatController combat =
            Instantiate(classData.combatPrefab, player.transform);

        combat.transform.localPosition = Vector3.zero;
        combat.transform.localRotation = Quaternion.identity;

        // Apply base stats
        var stats = player.GetComponent<PlayerStats>();
        stats.SetBaseStats(
            classData.baseHealth,
            classData.baseStamina,
            classData.baseDamage,
            0
        );

        // Inject stats
        combat.SetBaseDamage(classData.baseDamage);

        // ---------- APPLY HEALTH AFTER STATS ----------
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ForceRefresh(); // safer than ApplyStats(true)
        }

    }

    private void EnsureRoomAccessibleTiles(Room room)
    {
        if (room == null) return;

        // If already built and non-empty, keep it
        if (room.PositionsAccessibleFromPath != null && room.PositionsAccessibleFromPath.Count > 0)
            return;

        // Try BFS from entry
        if (TryGetRoomEntry(room, out var entryTile))
        {
            var graph = new RoomGraph(room.FloorTiles);
            Dictionary<Vector2Int, int> visited = graph.RunBFS(entryTile, GetRoomOccupied(room));
            room.PositionsAccessibleFromPath = visited.Keys.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        // If still empty, fallback to entire floor
        if (room.PositionsAccessibleFromPath == null || room.PositionsAccessibleFromPath.Count == 0)
        {
            room.PositionsAccessibleFromPath = room.FloorTiles
                .Where(t => !reserved.Contains(t))
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
        }
    }

    private HashSet<Vector2Int> GetRoomOccupied(Room room)
    {
        // Combine global reserved + room props (room props should already be in reserved, but safe)
        var occ = new HashSet<Vector2Int>(reserved);
        if (room?.PropPositions != null) occ.UnionWith(room.PropPositions);
        return occ;
    }

    private bool TryGetRoomEntry(Room room, out Vector2Int entry)
    {
        if (room?.FloorTiles == null)
        {
            entry = default;
            return false;
        }

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
    // SMART SPAWNING (ROOMS)
    // ---------------------------

    private void PlaceEnemiesSmart(Room room, int enemyCount, float difficulty)
    {
        if (room == null) return;
        if (dungeonData.PlayerReference == null) return;
        if (room.PositionsAccessibleFromPath == null || room.PositionsAccessibleFromPath.Count == 0) return;

        // Candidates: room accessible tiles excluding reserved
        var candidates = room.PositionsAccessibleFromPath
            .Where(t => !reserved.Contains(t))
            .Distinct()
            .ToList();

        if (candidates.Count == 0) return;

        // Score tiles: prefer “middle-ish” tiles by distance from wall, but keep some randomness
        candidates = candidates
            .OrderByDescending(t =>
            {
                float rnd = UnityEngine.Random.value;
                float central = DistanceFromWall(room, t) / 4f; // 0.25 edge .. 1 center-ish
                float mixed = Mathf.Lerp(central, 1f - central, edgeBias); // edgeBias pushes slightly toward edges if wanted
                return rnd + mixed;
            })
            .ToList();

        // Spread tiles using farthest-point sampling from centroid (prevents corner clumps)
        List<Vector2Int> chosen = PickFarthestPointSpread(candidates, enemyCount, minSpacing);

        // Fill if still short
        if (chosen.Count < enemyCount)
            chosen.AddRange(candidates.Except(chosen).Take(enemyCount - chosen.Count));

        float eliteChance = Mathf.Lerp(elitePackChanceBase, elitePackChanceMax,
            Mathf.InverseLerp(1f, 3f, difficulty));

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

            // Group
            EnemyTypeSO groupType = PickTypeWithAntiStreak(lastType);
            if (groupType != null &&
                groupType.prefersGroups &&
                UnityEngine.Random.value < groupType.groupChance &&
                remaining >= 2)
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

    private int DistanceFromWall(Room room, Vector2Int tile)
    {
        // Cheap "centrality": count how many 4-neighbours exist.
        // 4 = interior-ish, 1/2 = boundary-ish
        int missing = 0;
        if (!room.FloorTiles.Contains(tile + Vector2Int.up)) missing++;
        if (!room.FloorTiles.Contains(tile + Vector2Int.down)) missing++;
        if (!room.FloorTiles.Contains(tile + Vector2Int.left)) missing++;
        if (!room.FloorTiles.Contains(tile + Vector2Int.right)) missing++;
        return 4 - missing;
    }

    // ---------------------------
    // CORRIDOR SPAWNS
    // ---------------------------

    private void SpawnCorridorEnemies(int count)
    {
        if (count <= 0) return;

        var corridorCandidates = GetCorridorCandidates();
        if (corridorCandidates.Count == 0) return;

        // Spread along corridors
        var picked = PickFarthestPointSpread(corridorCandidates, count, corridorMinSpacing);

        foreach (var tile in picked)
        {
            // If already taken, skip
            if (reserved.Contains(tile)) continue;

            SpawnSingleIntoTile(tile, difficulty: 1f);
            reserved.Add(tile);
        }
    }

    private List<Vector2Int> GetCorridorCandidates()
    {
        // True corridor tiles (path), not reserved
        return dungeonData.path
            .Where(t => !reserved.Contains(t))
            .Distinct()
            .ToList();
    }

    private void SpawnSingleIntoTile(Vector2Int tile, float difficulty)
    {
        EnemyTypeSO type = WeightedRandoms.Pick(enemyTypes, t => t != null ? t.spawnWeight : 0f);
        if (type == null) return;

        int vIndex = PickVariantIndex(type, difficulty);
        SpawnEnemyGlobal(tile, type, (EnemyVariant)vIndex);
    }

    private void SpawnEnemyGlobal(Vector2Int tile, EnemyTypeSO type, EnemyVariant variant)
    {
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = (Vector2)tile + Vector2.one * 0.5f;

        var controller = enemy.GetComponent<EnemyController>();
        if (controller == null)
        {
            Debug.LogError("Enemy prefab missing EnemyController", enemy);
            Destroy(enemy);
            return;
        }

        controller.Init(type, variant, dungeonData.PlayerReference.transform);
    }

    // ---------------------------
    // TYPE PICKING (ANTI-STREAK)
    // ---------------------------

    private EnemyTypeSO PickTypeWithAntiStreak(EnemyTypeSO lastType)
    {
        return WeightedRandoms.Pick(enemyTypes, t =>
        {
            if (t == null) return 0f;
            float w = Mathf.Max(0f, t.spawnWeight);
            if (lastType != null && t == lastType) w *= 0.55f; // reduce streaks
            return w;
        });
    }

    // ---------------------------
    // SPAWN HELPERS (ROOM)
    // ---------------------------

    private void SpawnSingle(Room room, Vector2Int tile, float difficulty, ref EnemyTypeSO lastType)
    {
        if (reserved.Contains(tile)) return;

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
        // Local positions near anchor
        var local = candidates
            .Where(t => Manhattan(t, anchor) <= Mathf.CeilToInt(groupRadius))
            .Where(t => !reserved.Contains(t))
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        if (local.Count == 0 && !reserved.Contains(anchor))
            local.Add(anchor);

        int spawned = 0;

        for (int i = 0; i < groupSize && local.Count > 0; i++)
        {
            var tile = local[0];
            local.RemoveAt(0);

            if (reserved.Contains(tile)) continue;

            int vIndex = PickVariantIndex(type, difficulty * 0.9f);
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
            .Where(t => Manhattan(t, anchor) <= Mathf.CeilToInt(groupRadius + 1))
            .Where(t => !reserved.Contains(t))
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        if (local.Count == 0 && !reserved.Contains(anchor))
            local.Add(anchor);

        int spawned = 0;

        // Elite leader
        if (local.Count > 0)
        {
            int eliteVariant = PickEliteVariantIndex(type);
            var leaderTile = local[0];
            local.RemoveAt(0);

            if (!reserved.Contains(leaderTile))
            {
                SpawnEnemy(room, leaderTile, type, (EnemyVariant)eliteVariant);
                spawned++;
            }
        }

        // Minions
        for (int i = 1; i < total && local.Count > 0; i++)
        {
            var tile = local[0];
            local.RemoveAt(0);

            if (reserved.Contains(tile)) continue;

            int vIndex = PickVariantIndex(type, difficulty * 0.8f);
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
        if (reserved.Contains(tile)) return;

        GameObject enemy = Instantiate(enemyPrefab);
        enemy.transform.position = (Vector2)tile + Vector2.one * 0.5f;

        var controller = enemy.GetComponent<EnemyController>();
        if (controller == null)
        {
            Debug.LogError("Enemy prefab missing EnemyController", enemy);
            Destroy(enemy);
            return;
        }

        controller.Init(type, variant, dungeonData.PlayerReference.transform);

        // Reserve AFTER init to prevent overlaps even if later spawns happen same frame
        reserved.Add(tile);

        room?.EnemiesInTheRoom?.Add(enemy);
        room?.PropPositions?.Add(tile); // optional: reserve in room too
    }

    // ---------------------------
    // VARIANT WEIGHTS + DIFFICULTY
    // ---------------------------

    private int PickVariantIndex(EnemyTypeSO type, float difficulty)
    {
        if (type == null || type.variants == null || type.variants.Length < 3)
            return UnityEngine.Random.Range(0, 3);

        // Base weights
        float w0 = Mathf.Max(0f, type.variants[0] != null ? type.variants[0].spawnWeight : 0f);
        float w1 = Mathf.Max(0f, type.variants[1] != null ? type.variants[1].spawnWeight : 0f);
        float w2 = Mathf.Max(0f, type.variants[2] != null ? type.variants[2].spawnWeight : 0f);

        if (w0 + w1 + w2 <= 0f)
            return UnityEngine.Random.Range(0, 3);

        // Difficulty bias toward stronger variants later
        float t = Mathf.InverseLerp(1f, 3f, difficulty); // 0..1
        w0 *= Mathf.Lerp(1f, 0.60f, t);
        w1 *= Mathf.Lerp(1f, 1.25f, t);
        w2 *= Mathf.Lerp(1f, 1.85f, t);

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

    // ---------------------------
    // SPREAD PICKING (NO CORNER CLUMPS)
    // ---------------------------

    private List<Vector2Int> PickFarthestPointSpread(List<Vector2Int> candidates, int count, int spacing)
    {
        var result = new List<Vector2Int>();
        if (candidates == null || candidates.Count == 0 || count <= 0) return result;

        // Filter out reserved
        var pool = candidates.Where(t => !reserved.Contains(t)).Distinct().ToList();
        if (pool.Count == 0) return result;

        // Start near centroid (middle-ish)
        Vector2 centroid = Vector2.zero;
        foreach (var p in pool) centroid += (Vector2)p;
        centroid /= pool.Count;

        Vector2Int start = pool.OrderBy(p => Vector2.Distance((Vector2)p, centroid)).First();
        result.Add(start);

        // Farthest-point sampling with spacing
        int relax = spacing;
        while (result.Count < count)
        {
            Vector2Int best = default;
            int bestMinD = -1;
            bool found = false;

            foreach (var c in pool)
            {
                if (result.Contains(c)) continue;

                int minD = int.MaxValue;
                for (int i = 0; i < result.Count; i++)
                    minD = Mathf.Min(minD, Manhattan(c, result[i]));

                if (minD < relax) continue;

                if (minD > bestMinD)
                {
                    bestMinD = minD;
                    best = c;
                    found = true;
                }
            }

            if (!found)
            {
                relax--;
                if (relax < 0) break;
                continue;
            }

            result.Add(best);
        }

        return result;
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
// Weighted random helper
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
