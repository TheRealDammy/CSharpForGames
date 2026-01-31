using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStatsSave
{
    public List<PlayerStatType> keys = new();
    public List<int> values = new();

    public Dictionary<PlayerStatType, int> statLevels
    {
        get
        {
            var dict = new Dictionary<PlayerStatType, int>();
            for (int i = 0; i < keys.Count; i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }

    public PlayerStatsSave(Dictionary<PlayerStatType, StatData> stats)
    {
        foreach (var pair in stats)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value.level);
        }
    }
}
