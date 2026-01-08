using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomDataExtractor : MonoBehaviour
{
    private DungeonData dungeonData;

    [SerializeField]
    private bool showGizmo = false;


    public UnityEvent OnFinishedRoomProcessing;

    private void Awake()
    {
        dungeonData = FindFirstObjectByType<DungeonData>();
    }
    public void ProcessRooms()
    {
        if (dungeonData == null)
        {
            dungeonData = FindFirstObjectByType<DungeonData>();
        }          
        
        if (dungeonData == null)
        {
            Debug.Log($"Extractor sees rooms: {dungeonData.rooms.Count}, path tiles: {dungeonData.path.Count}");
            return;
        }
            

        foreach (Room room in dungeonData.rooms)
        {
            //find corener, near wall and inner tiles
            foreach (Vector2Int tilePosition in room.FloorTiles)
            {
                int neighboursCount = 4;

                if (room.FloorTiles.Contains(tilePosition + Vector2Int.up) == false)
                {
                    room.NearWallTilesUp.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.down) == false)
                {
                    room.NearWallTilesDown.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.right) == false)
                {
                    room.NearWallTilesRight.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.left) == false)
                {
                    room.NearWallTilesLeft.Add(tilePosition);
                    neighboursCount--;
                }

                //find corners
                bool up = room.FloorTiles.Contains(tilePosition + Vector2Int.up);
                bool down = room.FloorTiles.Contains(tilePosition + Vector2Int.down);
                bool left = room.FloorTiles.Contains(tilePosition + Vector2Int.left);
                bool right = room.FloorTiles.Contains(tilePosition + Vector2Int.right);

                int neighbourCount = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

                // Diagonals in each quadrant
                bool upLeftDiag = room.FloorTiles.Contains(tilePosition + Vector2Int.up + Vector2Int.left);
                bool upRightDiag = room.FloorTiles.Contains(tilePosition + Vector2Int.up + Vector2Int.right);
                bool downLeftDiag = room.FloorTiles.Contains(tilePosition + Vector2Int.down + Vector2Int.left);
                bool downRightDiag = room.FloorTiles.Contains(tilePosition + Vector2Int.down + Vector2Int.right);

                // Convex L-corner = exactly 2 neighbours in an L-shape AND the diagonal outside is empty
                bool isConvexCorner =
                    neighbourCount == 2 &&
                    (
                        (up && left && !upLeftDiag) ||
                        (up && right && !upRightDiag) ||
                        (down && left && !downLeftDiag) ||
                        (down && right && !downRightDiag)
                    );

                if (isConvexCorner)
                    room.CornerTiles.Add(tilePosition);

                if (neighbourCount == 4)
                    room.InnerTiles.Add(tilePosition);
            }

            room.NearWallTilesUp.ExceptWith(room.CornerTiles);
            room.NearWallTilesDown.ExceptWith(room.CornerTiles);
            room.NearWallTilesLeft.ExceptWith(room.CornerTiles);
            room.NearWallTilesRight.ExceptWith(room.CornerTiles);
        }
    }

    public void RunEvent()
    {
        OnFinishedRoomProcessing?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {      
        if (dungeonData == null || showGizmo == false)
            return;

        Gizmos.color = Color.red;
        foreach (var p in dungeonData.path)
        {
            Gizmos.DrawCube(p + Vector2.one * 0.5f, Vector3.one);
        }

        foreach (Room room in dungeonData.rooms)
        {
            //Draw inner tiles
            Gizmos.color = Color.yellow;
            foreach (Vector2Int floorPosition in room.InnerTiles)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
            //Draw near wall tiles UP
            Gizmos.color = Color.blue;
            foreach (Vector2Int floorPosition in room.NearWallTilesUp)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
            //Draw near wall tiles DOWN
            Gizmos.color = Color.green;
            foreach (Vector2Int floorPosition in room.NearWallTilesDown)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
            //Draw near wall tiles RIGHT
            Gizmos.color = Color.white;
            foreach (Vector2Int floorPosition in room.NearWallTilesRight)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
            //Draw near wall tiles LEFT
            Gizmos.color = Color.cyan;
            foreach (Vector2Int floorPosition in room.NearWallTilesLeft)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
            //Draw near wall tiles CORNERS
            Gizmos.color = Color.magenta;
            foreach (Vector2Int floorPosition in room.CornerTiles)
            {
                if (dungeonData.path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector3.one);
            }
        }
    }
}