using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatType
{
    Health,
    Stamina,
    Strength,
    Durability
}

[Serializable]
public class StatData
{
    public int level;
    public int softCap;
    public int hardCap;
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private Dictionary<PlayerStatType, StatData> stats;

    public event Action<PlayerStatType> OnStatChanged;
    public event Action OnStatsLoaded;

    private const string SAVE_KEY = "PLAYER_STATS_V1";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaults();
        LoadStats();
    }

    public void InitializeDefaults()
    {
        stats = new Dictionary<PlayerStatType, StatData>
        {
            { PlayerStatType.Health,     new StatData { level = 0, softCap = 10, hardCap = 20 } },
            { PlayerStatType.Stamina,    new StatData { level = 0, softCap = 10, hardCap = 20 } },
            { PlayerStatType.Strength,   new StatData { level = 0, softCap = 8,  hardCap = 15 } },
            { PlayerStatType.Durability, new StatData { level = 0, softCap = 8,  hardCap = 15 } }
        };
    }

    // ---------------- READ ----------------
    public int GetStatLevel(PlayerStatType type)
    {
        if (stats == null || !stats.ContainsKey(type))
            return 0;

        return stats[type].level;
    }

    public int GetStatSoftCap(PlayerStatType type)
    {
        if (stats == null || !stats.ContainsKey(type))
            return 0;
        return stats[type].softCap;
    }      

    public int GetStatHardCap(PlayerStatType type)
    {
        if (stats == null || !stats.ContainsKey(type))
            return 0;
        return stats[type].hardCap;
    }

    public bool IsAtHardCap(PlayerStatType type)
    {
        return GetStatLevel(type) >= stats[type].hardCap;
    }

    public StatData GetStatData(PlayerStatType type)
    {
        if (stats == null || !stats.ContainsKey(type))
            return null;
        return stats[type];
    }

    public string GetUpgradePreview(PlayerStatType type)
    {
        switch (type)
        {
            case PlayerStatType.Health:
                return "+20 Max Health";

            case PlayerStatType.Stamina:
                return "+10 Max Stamina";

            case PlayerStatType.Strength:
                return "+1 Damage";

            case PlayerStatType.Durability:
                return "+4% Damage Reduction";

            default:
                return "";
        }
    }


    // ---------------- WRITE ----------------
    public bool TryIncreaseStat(PlayerStatType type)
    {
        if (!stats.ContainsKey(type)) return false;

        var stat = stats[type];
        if (stat.level >= stat.hardCap) return false;

        stat.level++;
        SaveStats();
        OnStatChanged?.Invoke(type);
        return true;
    }

    public bool TryDecreaseStat(PlayerStatType type)
    {
        if (!stats.ContainsKey(type)) return false;
        if (stats[type].level <= 0) return false;

        stats[type].level--;
        SaveStats();
        OnStatChanged?.Invoke(type);
        return true;
    }

    // ---------------- SAVE / LOAD ----------------
    private void SaveStats()
    {
        var save = new PlayerStatsSave(stats);
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(save));
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            OnStatsLoaded?.Invoke();
            return;
        }

        var json = PlayerPrefs.GetString(SAVE_KEY);
        var save = JsonUtility.FromJson<PlayerStatsSave>(json);

        foreach (var pair in save.statLevels)
        {
            if (stats.ContainsKey(pair.Key))
                stats[pair.Key].level = Mathf.Clamp(
                    pair.Value,
                    0,
                    stats[pair.Key].hardCap
                );
        }

        OnStatsLoaded?.Invoke();
    }
}
