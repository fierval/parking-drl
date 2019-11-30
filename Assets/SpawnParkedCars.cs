using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

using Random = UnityEngine.Random;

public class SpawnParkedCars : MonoBehaviour
{
    Dictionary<GameObject, List<Vector3>> parkingSpots = new Dictionary<GameObject, List<Vector3>>();
    List<GameObject> instantiatedCars = new List<GameObject>();

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
            // get cars
            var prefabs = Enumerable.Range(0, carPrefabs.Length)
                .Select(_ => carPrefabs[Random.Range(0, carPrefabs.Length - 1)])
                .Take(maxOccupiedSpaces)
                .ToList();

            var spots = Enumerable.Range(0, parkingSpots[lot].Count)
                .Select(_ => parkingSpots[lot][Random.Range(0, parkingSpots[lot].Count - 1)])
                .Take(maxOccupiedSpaces)
                .ToList();

            // rotation angles for y axis: 90 or -90
            var angles = Enumerable.Range(0, maxOccupiedSpaces)
                .Select(_ => 90 * (Random.Range(0, 1) * 2 - 1))
                .ToList();

           for(int i = 0; i < maxOccupiedSpaces; i++)
           {
               instantiatedCars.Add(Instantiate(prefabs[i], spots[i], Quaternion.AngleAxis(angles[i], transform.up)));
           }
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

    void DestroyCars()
    {
        if(instantiatedCars?.Count == 0)
        {
            return;
        }

        foreach (var car in instantiatedCars)
        {
                Destroy(car);
        }
        instantiatedCars.Clear();
        parkingSpots.Clear();
    }
}
