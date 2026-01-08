using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class WallGenerator
{
    private static readonly Vector2Int[] CardinalDirs =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TileMapGenerator tileMapGenerator)
    {
        // NEW: compute which empty tiles are truly "outside"
        HashSet<Vector2Int> outsideEmpty = ComputeOutsideEmpty(floorPositions);

        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList, outsideEmpty);
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList, outsideEmpty);

        CreateBasicWalls(tileMapGenerator, basicWallPositions, floorPositions);
        CreateCornerWalls(tileMapGenerator, cornerWallPositions, floorPositions);
    }

    private static void CreateCornerWalls(TileMapGenerator tileMapGenerator, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in cornerWallPositions)
        {
            bool hasCardinalFloor =
            floorPositions.Contains(position + Vector2Int.up) ||
            floorPositions.Contains(position + Vector2Int.down) ||
            floorPositions.Contains(position + Vector2Int.left) ||
            floorPositions.Contains(position + Vector2Int.right);

            if (!hasCardinalFloor)
                continue;
        }

        foreach (var position in cornerWallPositions)
        {
            string neighborBinaryType = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighborPosition = position + direction;
                neighborBinaryType += floorPositions.Contains(neighborPosition) ? "1" : "0";
            }
            tileMapGenerator.PaintSingleCornerWall(position, neighborBinaryType);
        }
    }

    private static void CreateBasicWalls(TileMapGenerator tileMapGenerator, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPositions)
        {
            string neighborBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighborPosition = position + direction;
                neighborBinaryType += floorPositions.Contains(neighborPosition) ? "1" : "0";
            }
            tileMapGenerator.PaintSingleWall(position, neighborBinaryType);
        }
    }

    // only add neighbour if it's OUTSIDE empty (prevents interior holes walls)
    private static HashSet<Vector2Int> FindWallsInDirections(
        HashSet<Vector2Int> floorPositions,
        List<Vector2Int> directionList,
        HashSet<Vector2Int> outsideEmpty)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighborPosition = position + direction;

                if (!floorPositions.Contains(neighborPosition) && outsideEmpty.Contains(neighborPosition))
                {
                    wallPositions.Add(neighborPosition);
                }
            }
        }
        return wallPositions;
    }

    // NEW: Flood fill from outside of floor bounds
    private static HashSet<Vector2Int> ComputeOutsideEmpty(HashSet<Vector2Int> floor)
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var p in floor)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        // expand bounds so we have guaranteed "outside" area
        minX -= 2; minY -= 2; maxX += 2; maxY += 2;

        bool InBounds(Vector2Int p) => p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY;

        Vector2Int start = new Vector2Int(minX, minY);

        var outside = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();

        // start tile must be empty
        q.Enqueue(start);
        outside.Add(start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();

            foreach (var d in CardinalDirs)
            {
                var nxt = cur + d;
                if (!InBounds(nxt)) continue;
                if (outside.Contains(nxt)) continue;
                if (floor.Contains(nxt)) continue; // can't pass through floor

                outside.Add(nxt);
                q.Enqueue(nxt);
            }
        }

        return outside;
    }


    public static void FillEnclosedHoles(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> outsideEmpty = ComputeOutsideEmpty(floorPositions);

        // Bounds of floor (+2 margin, same as ComputeOutsideEmpty)
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var p in floorPositions)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }
        minX -= 2; minY -= 2; maxX += 2; maxY += 2;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var pos = new Vector2Int(x, y);

                // If it's not floor and NOT reachable from outside, it's a hole → fill it
                if (!floorPositions.Contains(pos) && !outsideEmpty.Contains(pos))
                    floorPositions.Add(pos);
            }
        }
    }
}