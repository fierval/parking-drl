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
    public const string CarTag = "car";
    public const string ImmovableTag = "immovable";
    public const string Marker = "Marker";
    private static Random rng = new Random();

    public const string RandomSpot = "random_spot";
    public const string NumCars = "num_cars";
    public const string GoalSpot = "goal_spot";

    private static float Round1(this float f)
    {
        return (float) Math.Round(f, 1);
    }

    /// <summary>
    /// Does "container" contain "rect"
    /// </summary>
    /// <param name="container"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static bool Contains(this Bounds container, Bounds rect)
    {
        return container.min.x.Round1() <= rect.min.x.Round1()
            && container.min.z.Round1() <= rect.min.z.Round1()
            && container.max.x.Round1() >= rect.max.x.Round1()
            && container.max.z.Round1() >= rect.max.z.Round1();
    }

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
