# 2D Procedural Dungeon Crawler (Unity)

Top-down 2D dungeon crawler prototype in Unity, focused on **procedural dungeon generation** and scalable gameplay foundations.

## Current milestone
✅ Procedural dungeon generation implemented (rooms + corridors, fully navigable layouts)

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
