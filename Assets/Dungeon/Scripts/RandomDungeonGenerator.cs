using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField] protected SimpleRandomDungeonData RandomDungeonData;

    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalks(RandomDungeonData, startPosition);
        tileMapGenerator.Clear();      
        tileMapGenerator.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapGenerator);
    }

    protected HashSet<Vector2Int> RunRandomWalks(SimpleRandomDungeonData data, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < RandomDungeonData.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.RandomWalk(currentPosition, RandomDungeonData.walkLength);
            floorPositions.UnionWith(path);
            if (RandomDungeonData.startRandomly)
            {
                currentPosition = floorPositions.ElementAt(UnityEngine.Random.Range(0, floorPositions.Count));
            }
        }

        return floorPositions;
    }
}
