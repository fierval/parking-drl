﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Flags]
public enum ParkingState : int
{
    Available = 0,
    InProgress = 1,
    Incomplete = 2,
    Complete = 4
}

public static class ParkingUtils
{
    /// <summary>
    /// Create rectangle from local bounds
    /// </summary>
    /// <param name="transform">game object transform</param>
    /// <returns>rectangle in XZ plane in world coordinates</returns>
    public static Rect CreateRectFromTransformAndLocalBounds(Transform transform)
    {
        Bounds localBounds = transform.gameObject.GetComponent<MeshFilter>().mesh.bounds;

        var min = transform.TransformVector(localBounds.min);
        var max = transform.TransformVector(localBounds.max);

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    /// <summary>
    /// Does "container" contain "rect"
    /// </summary>
    /// <param name="container"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static bool Contains(this Rect container, Rect rect)
    {
        return container.min.x <= rect.min.x
            && container.min.y <= rect.min.y
            && container.max.x >= rect.max.y
            && container.max.y >= rect.max.y;
    }


}