using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

using Random = UnityEngine.Random;

public class SpawnParkedCars : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> parkingSpots = new Dictionary<GameObject, List<GameObject>>();
    List<GameObject> instantiatedCars = new List<GameObject>();

    [SerializeField] GameObject[] carPrefabs;
    [SerializeField] GameObject[] parkingLots;

    [SerializeField] int maxOccupiedSpaces;
    
    [SerializeField, Tooltip("Should we populate parking spots on activation")] bool spawnOnAwake;
    readonly HashSet<GameObject> allSpots = new HashSet<GameObject>();

    public IEnumerable<GameObject> FreeSpots
    {
        get
        {
            var occupied = parkingSpots.SelectMany(d => d.Value);
            return allSpots.Except(occupied);
        }
    }

    private void Awake()
    {
        if(spawnOnAwake)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        DestroyCars();
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
               instantiatedCars.Add(Instantiate(prefabs[i], 
                   spots[i].transform.position, 
                   Quaternion.AngleAxis(angles[i], transform.up)));
           }
        }


    }

    private void PopulateFreeSpots()
    {
        // Populate the list of parking spots
        foreach (var pl in parkingLots)
        {
            parkingSpots.Add(pl, new List<GameObject>());
            foreach (Transform spot in pl.transform.GetChild(0) as Transform)
            {
                parkingSpots[pl].Add(spot.gameObject);
                allSpots.Add(spot.gameObject);
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
