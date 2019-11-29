using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

using Random = UnityEngine.Random;

public class SpawnParkedCars : MonoBehaviour
{
    Dictionary<GameObject, List<Vector3>> parkingSpots;
    [SerializeField] GameObject[] carPrefabs;
    [SerializeField] GameObject[] parkingLots;
    [SerializeField] int maxOccupiedSpaces; 

    private void Awake()
    {
        PopulateFreeSpots();
        OccupyParkingSpots();
    }

    private void OccupyParkingSpots()
    {
        foreach (var lot in parkingLots)
        {
            var prefabs = Enumerable.Range(0, maxOccupiedSpaces)
                .Select(_ => parkingSpots[lot][Random.Range(0, carPrefabs.Length)])
                .ToList();
        }


    }

    private void PopulateFreeSpots()
    {
        // Populate the list of parking spots
        foreach (var pl in parkingLots)
        {
            parkingSpots.Add(pl, new List<Vector3>());
            foreach (Transform spot in pl.transform.GetChild(0) as Transform)
            {
                parkingSpots[pl].Add(spot.position);
            }
        }
    }
}
