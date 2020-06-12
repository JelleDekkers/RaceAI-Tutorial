using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static T GetRandom<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
