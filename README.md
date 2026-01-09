# 2D Procedural Dungeon Crawler (Unity)

Top-down 2D dungeon crawler prototype in Unity, focused on **procedural dungeon generation** and scalable gameplay foundations.

## Current milestone
✅ Procedural dungeon generation implemented (rooms + corridors, fully navigable layouts)

## Procedural Generation (Random Walk + BSP)

This project supports two generation styles:

- **Random Walk**: carves organic cave-like layouts by randomly walking on a grid and marking tiles as floor.
- **BSP (Binary Space Partitioning)**: splits the map into partitions, places rooms, and connects them with corridors for a structured dungeon layout.

Both pipelines convert the result into floor/wall tile data and render it to **Unity Tilemaps**.
The goal is “controlled randomness”: different runs, but still reliable for gameplay and future systems (enemies, loot, encounters).


## How the generation works (plain English)
The dungeon is built in stages:
1. Place rooms on a grid
2. Connect rooms with corridors so the dungeon is always playable
3. Convert the layout into floor/wall tile data
4. Render to Unity Tilemaps

Goal: **controlled randomness** — each run is different, but the dungeon stays reliable for gameplay.

## Tech
- Unity 2D + Tilemaps
- C#

## Next steps
- Player controller + combat loop
- Enemy spawning + simple AI
- Loot drops / pickups
- Encounter rooms
