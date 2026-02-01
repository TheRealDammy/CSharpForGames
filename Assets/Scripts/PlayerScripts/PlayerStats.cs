using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatData
{
    public int level;
    public int softCap;
    public int hardCap;
}

public class PlayerStats : MonoBehaviour
{
    private Dictionary<PlayerStatType, StatData> stats;

    private Dictionary<PlayerStatType, int> baseStats = new();

    public event Action<PlayerStatType> OnStatChanged;

    private void Awake()
    {
        stats = new()
        {
            { PlayerStatType.Health,     new StatData{ level=0, softCap=10, hardCap=20 }},
            { PlayerStatType.Stamina,    new StatData{ level=0, softCap=10, hardCap=20 }},
            { PlayerStatType.Strength,   new StatData{ level=0, softCap=8,  hardCap=15 }},
            { PlayerStatType.Durability, new StatData{ level=0, softCap=8,  hardCap=15 }}
        };
    }

    public void SetBaseStats(int hp, int stam, int str, int dur)
    {
        baseStats[PlayerStatType.Health] = hp;
        baseStats[PlayerStatType.Stamina] = stam;
        baseStats[PlayerStatType.Strength] = str;
        baseStats[PlayerStatType.Durability] = dur;
    }
    private void EnsureInitialized()
    {
        if (stats != null) return;

        stats = new()
        {
            { PlayerStatType.Health,     new StatData{ level=0, softCap=10, hardCap=20 }},
            { PlayerStatType.Stamina,    new StatData{ level=0, softCap=10, hardCap=20 }},
            { PlayerStatType.Strength,   new StatData{ level=0, softCap=8,  hardCap=15 }},
            { PlayerStatType.Durability, new StatData{ level=0, softCap=8,  hardCap=15 }}
        };
    }
    public void ApplyClassDefaults(CharacterClass cls)
    {
        EnsureInitialized();

        switch (cls)
        {
            case CharacterClass.Swordsman:
                stats[PlayerStatType.Health].level += 0;
                stats[PlayerStatType.Strength].level += 0;
                break;

            case CharacterClass.Archer:
                stats[PlayerStatType.Stamina].level += 0;
                stats[PlayerStatType.Strength].level += 0;
                break;

            case CharacterClass.Mage:
                stats[PlayerStatType.Stamina].level += 0;
                break;
        }

        OnStatChanged?.Invoke(PlayerStatType.Health);
        OnStatChanged?.Invoke(PlayerStatType.Stamina);
        OnStatChanged?.Invoke(PlayerStatType.Strength);
    }


    public int GetStatLevel(PlayerStatType type)
    {
        EnsureInitialized();
        return stats.TryGetValue(type, out var stat) ? stat.level : 0;
    }
    public int GetBaseStat(PlayerStatType type) 
    {
        EnsureInitialized();
        return baseStats[type];
    }
    public int GetFinalStat(PlayerStatType type)
    {
        EnsureInitialized();
        return baseStats[type] + stats[type].level * 20;
    }
    public bool TryIncreaseStat(PlayerStatType type)
    {
        if (stats[type].level >= stats[type].hardCap) return false;
        Math.Clamp(stats[type].level, 0, stats[type].hardCap);
        stats[type].level++;    
        OnStatChanged?.Invoke(type);
        return true;
    }

    public bool TryDecreaseStat(PlayerStatType type)
    {
        if (stats[type].level >= stats[type].hardCap) return false;
        Math.Clamp(stats[type].level, 0, stats[type].hardCap);
        stats[type].level--;   
        OnStatChanged?.Invoke(type);
        return true;
    }

    public int GetDamageReductionPercent()
    {
        return GetStatLevel(PlayerStatType.Durability) * 4;
    }

    public StatData GetStatData(PlayerStatType type)
    {
        return stats[type];
    }

    public string GetUpgradePreview(PlayerStatType type)
    {
        return type switch
        {
            PlayerStatType.Health => "+20 HP",
            PlayerStatType.Stamina => "+15 Stamina",
            PlayerStatType.Strength => "+5 Damage",
            PlayerStatType.Durability => "+4% DR",
            _ => ""
        };
    }
}
