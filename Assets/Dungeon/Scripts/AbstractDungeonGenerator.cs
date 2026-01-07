using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField] protected TileMapGenerator tileMapGenerator = null;
    [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

    public void GenerateDungeon()
    {
        tileMapGenerator.Clear();
        RunProceduralGeneration();  
    }

    protected abstract void RunProceduralGeneration();
}
