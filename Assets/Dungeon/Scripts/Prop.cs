using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Prop : ScriptableObject
{
    [Header("Prop data:")]
    public Sprite PropSprite;
    /// <summary>
    /// Affects the collider size of the prop
    /// </summary>
    public Vector2Int PropSize = Vector2Int.one;

    [Space, Header("Placement type:")]
    public bool Corner;
    public bool NearWallUP;
    public bool NearWallDown;
    public bool NearWallRight;
    public bool NearWallLeft;
    public bool Inner;
    [Min(1)]
    public int PlacementQuantityMin = 1;
    [Min(1)]
    public int PlacementQuantityMax = 1;

    [Space, Header("Group placement:")]
    public bool PlaceAsGroup = false;
    [Min(1)]
    public int GroupMinCount = 1;
    [Min(1)]
    public int GroupMaxCount = 1;

    [Space, Header("Destructability:")]
    public bool Destructible;
    public int MaxHP = 1;
    public GameObject BreakVFXPrefab;

    [Space, Header("Spawn settings:")]
    [Range(0, 1f)] public float spawnChance = 1f;
    public bool hasColliders = true;
    public bool OnlyCorner;

    [Space, Header("Interactability:")]
    public bool Interactable;

    [Space, Header("Pickup Settings:")]
    public GameObject[] pickupPrefab; // Optional: prefab to spawn on break
    public float pickupSpawnChance = 0.2f; // 20% chance to spawn pickup
    public int numPickupsToSpawn = 1; // Number of pickups to spawn
}