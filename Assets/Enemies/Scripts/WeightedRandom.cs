using System;
using System.Collections.Generic;
using UnityEngine;

public static class WeightedRandom
{
    public static T Pick<T>(IList<T> items, Func<T, float> weight)
    {
        if (items == null || items.Count == 0) return default;

        float total = 0f;
        for (int i = 0; i < items.Count; i++)
            total += Mathf.Max(0f, weight(items[i]));

        if (total <= 0f)
            return items[UnityEngine.Random.Range(0, items.Count)];

        float r = UnityEngine.Random.value * total;
        for (int i = 0; i < items.Count; i++)
        {
            r -= Mathf.Max(0f, weight(items[i]));
            if (r <= 0f) return items[i];
        }
        return items[items.Count - 1];
    }
}
