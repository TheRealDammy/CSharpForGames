using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PropPlacementManager : MonoBehaviour
{
    DungeonData dungeonData;

    [SerializeField] private List<Prop> propsToPlace;
    [SerializeField] private GameObject propPrefab;

    public UnityEvent OnFinished;

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

        if (dungeonData == null || dungeonData.rooms == null) return;
        if (propsToPlace == null || propsToPlace.Count == 0) return;
        propsToPlace = propsToPlace.Where(p => p != null).ToList();

        foreach (Room room in dungeonData.rooms)
        {
            //Place props place props in the corners
            List<Prop> cornerProps = propsToPlace
                .Where(x => x.Corner)
                .ToList();
            PlaceCornerProps(room, cornerProps);

            //Place props near LEFT wall
            List<Prop> leftWallProps = propsToPlace
                .Where(p => p != null && p.NearWallLeft && !p.OnlyCorner)
                .ToList();


            PlaceProps(room, leftWallProps, room.NearWallTilesLeft, PlacementOriginCorner.BottomLeft);

            //Place props near RIGHT wall
            List<Prop> rightWallProps = propsToPlace
                .Where(p => p != null && p.NearWallRight && !p.OnlyCorner)
                .ToList();

            PlaceProps(room, rightWallProps, room.NearWallTilesRight, PlacementOriginCorner.TopRight);

            //Place props near UP wall
            List<Prop> topWallProps = propsToPlace
                .Where(p => p != null && p.NearWallUP && !p.OnlyCorner)
                .ToList();

            PlaceProps(room, topWallProps, room.NearWallTilesUp, PlacementOriginCorner.TopLeft);

            //Place props near DOWN wall
            List<Prop> downWallProps = propsToPlace
                .Where(p => p != null && p.NearWallDown && !p.OnlyCorner)
                .ToList();

            PlaceProps(room, downWallProps, room.NearWallTilesDown, PlacementOriginCorner.BottomLeft);

            //Place inner props
            List<Prop> innerProps = propsToPlace
                .Where(p => p != null && p.Inner && !p.OnlyCorner)
                .ToList();
            PlaceProps(room, innerProps, room.InnerTiles, PlacementOriginCorner.BottomLeft);
        }

        //OnFinished?.Invoke();
        Invoke("RunEvent", 1);

    }

    public void RunEvent()
    {
        OnFinished?.Invoke();
    }

    /// <summary>
    /// Places props near walls. We need to specify the props anw the placement start point
    /// </summary>
    /// <param name="room"></param>
    /// <param name="wallProps">Props that we should try to place</param>
    /// <param name="availableTiles">Tiles that are near the specific wall</param>
    /// <param name="placement">How to place bigger props. Ex near top wall we want to start placemt from the Top corner and find if there are free spaces below</param>
    private void PlaceProps(
        Room room, List<Prop> wallProps, HashSet<Vector2Int> availableTiles, PlacementOriginCorner placement)
    {
        //Remove path positions from the initial nearWallTiles to ensure the clear path to traverse dungeon
        HashSet<Vector2Int> tempPositons = new HashSet<Vector2Int>(availableTiles);
        tempPositons.ExceptWith(dungeonData.path);

        //We will try to place all the props
        foreach (Prop propToPlace in wallProps)
        {
            //We want to place only certain quantity of each prop
            int quantity
                = UnityEngine.Random.Range(propToPlace.PlacementQuantityMin, propToPlace.PlacementQuantityMax + 1);

            for (int i = 0; i < quantity; i++)
            {
                float chance = propToPlace.spawnChance;
                if (chance > 1f) chance /= 100f;      // allow 0–100 inputs
                chance = Mathf.Clamp01(chance);

                if (UnityEngine.Random.value > chance)
                    continue;


                //remove taken positions
                tempPositons.ExceptWith(room.PropPositions);
                //shuffel the positions
                List<Vector2Int> availablePositions = tempPositons.OrderBy(x => Guid.NewGuid()).ToList();
                //If placement has failed there is no point in trying to place the same prop again
                if (TryPlacingPropBruteForce(room, propToPlace, availablePositions, placement) == false)
                    break;
            }
        }
    }

    /// <summary>
    /// Tries to place the Prop using brute force (trying each available tile position)
    /// </summary>
    /// <param name="room"></param>
    /// <param name="propToPlace"></param>
    /// <param name="availablePositions"></param>
    /// <param name="placement"></param>
    /// <returns>False if there is no space. True if placement was successful</returns>
    private bool TryPlacingPropBruteForce(
        Room room, Prop propToPlace, List<Vector2Int> availablePositions, PlacementOriginCorner placement)
    {
        //try placing the objects starting from the corner specified by the placement parameter
        for (int i = 0; i < availablePositions.Count; i++)
        {
            //select the specified position (but it can be already taken after placing the corner props as a group)
            Vector2Int position = availablePositions[i];
            if (room.PropPositions.Contains(position))
                continue;

            //check if there is enough space around to fit the prop
            List<Vector2Int> freePositionsAround
                = TryToFitProp(propToPlace, availablePositions, position, placement);

            //If we have enough spaces place the prop
            if (freePositionsAround.Count == propToPlace.PropSize.x * propToPlace.PropSize.y)
            {
                //Place the gameobject
                Vector2Int anchor = new Vector2Int(
                    freePositionsAround.Min(p => p.x),
                    freePositionsAround.Min(p => p.y)
                );

                PlacePropGameObjectAt(room, anchor, propToPlace);

                foreach (Vector2Int pos in freePositionsAround)
                    room.PropPositions.Add(pos);

                //Deal with groups
                if (propToPlace.PlaceAsGroup)
                {
                    PlaceGroupObject(room, position, propToPlace, 1);
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the prop will fit (accordig to it size)
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="availablePositions"></param>
    /// <param name="originPosition"></param>
    /// <param name="placement"></param>
    /// <returns></returns>
    private List<Vector2Int> TryToFitProp(
        Prop prop,
        List<Vector2Int> availablePositions,
        Vector2Int originPosition,
        PlacementOriginCorner placement)
    {
        List<Vector2Int> freePositions = new();

        //Perform the specific loop depending on the PlacementOriginCorner
        if (placement == PlacementOriginCorner.BottomLeft)
        {
            for (int xOffset = 0; xOffset < prop.PropSize.x; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.PropSize.y; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.BottomRight)
        {
            for (int xOffset = -prop.PropSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.PropSize.y; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.TopLeft)
        {
            for (int xOffset = 0; xOffset < prop.PropSize.x; xOffset++)
            {
                for (int yOffset = -prop.PropSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else
        {
            for (int xOffset = -prop.PropSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = -prop.PropSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }

        return freePositions;
    }

    /// <summary>
    /// Places props in the corners of the room
    /// </summary>
    /// <param name="room"></param>
    /// <param name="cornerProps"></param>
    private void PlaceCornerProps(Room room, List<Prop> cornerProps)
    {
        if (cornerProps == null || cornerProps.Count == 0) return;

        // If you have OnlyCorner, prioritize those (chests)
        var priority = cornerProps
            .Where(p => p != null)
            .OrderByDescending(p => p.OnlyCorner)
            .ToList();

        foreach (var prop in priority)
        {
            float chance = prop.spawnChance;
            if (chance > 1f) chance /= 100f;
            chance = Mathf.Clamp01(chance);

            if (UnityEngine.Random.value > chance)
                continue;

            // Blob-friendly: place on boundary far from path
            if (TryPlaceChestLikeProp(room, prop))
            {
                // only one chest-like prop per room
                if (prop.OnlyCorner) break;
            }
        }
    }


    private int DistanceToClosestPath(Vector2Int from)
    {
        if (dungeonData == null || dungeonData.path == null || dungeonData.path.Count == 0)
            return int.MaxValue;

        int best = int.MaxValue;

        foreach (var p in dungeonData.path)
        {
            int dist = Mathf.Abs(from.x - p.x) + Mathf.Abs(from.y - p.y); // Manhattan distance
            if (dist < best)
                best = dist;
        }

        return best;
    }

    private List<Vector2Int> GetChestCandidates(Room room)
    {
        // Use near-wall tiles (boundary of blob). Combine all sides.
        var candidates = room.NearWallTilesUp
            .Union(room.NearWallTilesDown)
            .Union(room.NearWallTilesLeft)
            .Union(room.NearWallTilesRight)
            .ToList();

        // Filter out path + occupied
        candidates = candidates
            .Where(t => !dungeonData.path.Contains(t))
            .Where(t => !room.PropPositions.Contains(t))
            .ToList();

        return candidates;
    }

    private bool TryPlaceChestLikeProp(Room room, Prop chestProp)
    {
        var candidates = GetChestCandidates(room);
        if (candidates.Count == 0) return false;

        Vector2Int best = candidates
            .OrderByDescending(t => DistanceToClosestPath(t))
            .First();

        PlacePropGameObjectAt(room, best, chestProp);
        room.PropPositions.Add(best);
        return true;
    }

    /// <summary>
    /// Helps to find free spaces around the groupOriginPosition to place a prop as a group
    /// </summary>
    /// <param name="room"></param>
    /// <param name="groupOriginPosition"></param>
    /// <param name="propToPlace"></param>
    /// <param name="searchOffset">The search offset ex 1 = we will check all tiles withing the distance of 1 unity away from origin position</param>
    private void PlaceGroupObject(
        Room room, Vector2Int groupOriginPosition, Prop propToPlace, int searchOffset)
    {
        //*Can work poorely when placing bigger props as groups

        //calculate how many elements are in the group -1 that we have placed in the center
        int count = UnityEngine.Random.Range(propToPlace.GroupMinCount, propToPlace.GroupMaxCount) - 1;
        count = Mathf.Clamp(count, 0, 8);

        //find the available spaces around the center point.
        //we use searchOffset to limit the distance between those points and the center point
        List<Vector2Int> availableSpaces = new List<Vector2Int>();
        for (int xOffset = -searchOffset; xOffset <= searchOffset; xOffset++)
        {
            for (int yOffset = -searchOffset; yOffset <= searchOffset; yOffset++)
            {
                Vector2Int tempPos = groupOriginPosition + new Vector2Int(xOffset, yOffset);
                if (room.FloorTiles.Contains(tempPos) &&
                    !dungeonData.path.Contains(tempPos) &&
                    !room.PropPositions.Contains(tempPos))
                {
                    availableSpaces.Add(tempPos);
                }
            }
        }

        //shuffle the list
        availableSpaces = availableSpaces.OrderBy(x => Guid.NewGuid()).ToList();

        //place the props (as many as we want or if there is less space fill all the available spaces)
        int tempCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        for (int i = 0; i < tempCount; i++)
        {
            PlacePropGameObjectAt(room, availableSpaces[i], propToPlace);
        }

    }

    /// <summary>
    /// Place a prop as a new GameObject at a specified position
    /// </summary>
    /// <param name="room"></param>
    /// <param name="placementPostion"></param>
    /// <param name="propToPlace"></param>
    /// <returns></returns>
    private GameObject PlacePropGameObjectAt(Room room, Vector2Int placementPostion, Prop propToPlace)
    {
        //Instantiat the prop at this positon
        GameObject prop = Instantiate(propPrefab);
        SpriteRenderer propSpriteRenderer = prop.GetComponentInChildren<SpriteRenderer>();
        if (propSpriteRenderer == null)
        {
            Debug.LogError("Prop prefab has no SpriteRenderer in children.", prop);
            Destroy(prop);
            return null;
        }

        //set the sprite
        propSpriteRenderer.sprite = propToPlace.PropSprite;

        //Add a collider
        if (propToPlace.hasColliders)
        {
            CapsuleCollider2D collider
            = propSpriteRenderer.gameObject.AddComponent<CapsuleCollider2D>();
            collider.offset = Vector2.zero;
            if (propToPlace.PropSize.x > propToPlace.PropSize.y)
            {
                collider.direction = CapsuleDirection2D.Horizontal;
            }
            Vector2 size
                = new Vector2(propToPlace.PropSize.x * 0.8f, propToPlace.PropSize.y * 0.8f);
            collider.size = size;
        }
      
        prop.transform.position = (Vector2)placementPostion;
        //adjust the position to the sprite
        propSpriteRenderer.transform.localPosition
            = (Vector2)propToPlace.PropSize * 0.5f;

        //Save the prop in the room data (so in the dungeon data)
        room.PropObjectReferences.Add(prop);

        if (propToPlace.Destructible)
        {
            var destruct = prop.AddComponent<DestructibleProp>();
            destruct.Init(propToPlace, room, dungeonData, placementPostion);
        }

        if (propToPlace.Interactable)
        {
            var interactable = prop.AddComponent<InteractableProps>();
            interactable.Interact();
        }

        return prop;
    }
}

/// <summary>
/// Where to start placing the prop ex. start at BottomLeft corner and search 
/// if there are free space to the Right and Up in case of placing a biggex prop
/// </summary>
public enum PlacementOriginCorner
{
    BottomLeft,
    BottomRight,
    TopLeft,
    TopRight
}