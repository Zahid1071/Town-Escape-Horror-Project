using System;
using System.Collections.Generic;
using UnityEngine;

public static class WeightedPicker
{
    public static T Pick<T>(List<T> items, Func<T, float> getWeight)
    {
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += Mathf.Max(0f, getWeight(item));
        }

        float randomValue = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var item in items)
        {
            cumulative += Mathf.Max(0f, getWeight(item));
            if (randomValue <= cumulative)
                return item;
        }

        return items.Count > 0 ? items[items.Count - 1] : default;
    }
}
