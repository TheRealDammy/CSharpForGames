using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase[] floorTile;
    [SerializeField] private TileBase[] wallTop; 

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
         
    }
    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase[] tile)
    {
        foreach (var position in positions)
        {
            PaintSingleFloorTile(tilemap, tile, position);
        }
    }

    private void PaintSingleFloorTile(Tilemap tilemap, TileBase[] tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        var randomFloorTile = floorTile[Random.Range(0, floorTile.Length)];

        tilemap.SetTile(tilePosition, randomFloorTile);      
    }
    private void PaintSingleWallTile(Tilemap tilemap, TileBase[] tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        var randomWallTile = wallTop[Random.Range(0, wallTop.Length)];
        tilemap.SetTile(tilePosition, randomWallTile);
    }

    public void Clear()
    {
        wallTilemap.ClearAllTiles();
        floorTilemap.ClearAllTiles();
    }

    internal void PaintSingleWall(Vector2Int position)
    {
        PaintSingleWallTile(wallTilemap, wallTop, position);
    }
}
