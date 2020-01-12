using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public enum ParkingState : int
{
    Available = 0,
    InProgress = 1,
    Failed = 2,
    Complete = 3
}

public static class ParkingUtils
{
    static CarAcademy academy = GameObject.FindObjectOfType<CarAcademy>();

    public static bool IsAcademyActive() => academy != null && academy.enabled;
    public const string ParkingTag = "parking";
    
    private static Random rng = new Random();

    public static IEnumerable<T> Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
