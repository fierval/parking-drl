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

    [SerializeField, Range(0, 7)] int maxOccupiedSpaces;
    
    [SerializeField, Tooltip("Should we populate parking spots on activation")] bool spawnOnAwake;
    readonly HashSet<GameObject> allSpots = new HashSet<GameObject>();

    [SerializeField, Tooltip("Collider of this object is used to block out occupied parking spots")]

    List<GameObject> occupiedSpots = new List<GameObject>();
    List<GameObject> parkingBlockers = new List<GameObject>();

    /// <summary>
    /// Clone a car prefab at a fixed place
    /// </summary>
    /// <param name="car">car prefab</param>
    /// <param name="pos">position</param>
    /// <param name="rotation">rotation</param>
    public static GameObject CloneCar(GameObject car, Vector3 pos, Quaternion rotation)
    {
        GameObject carInstance = Instantiate(car, pos, rotation);

        carInstance.GetComponent<ESVehicleController>().enabled = false;
        carInstance.GetComponent<ESGearShift>().enabled = false;
        carInstance.GetComponent<ESAudioSystem>().enabled = false;
        carInstance.GetComponent<AudioSource>().enabled = false;
        carInstance.GetComponents<ESSpringBalance>().ToList().ForEach(c => c.enabled = false);
        carInstance.GetComponent<ParkingDetector>().enabled = false;

        carInstance.GetComponentsInChildren<AudioSource>().ToList()
            .ForEach(a => a.enabled = false);

        return carInstance;
    }

    public List<GameObject> FreeSpots
    {
        get
        {
            return allSpots.Except(occupiedSpots).ToList();
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
        DiscoverFreeSpots();
        OccupyParkingSpots();
    }
    private void OccupyParkingSpots()
    {
        foreach (var lot in parkingLots)
        {
            // get cars
            var prefabs = Enumerable.Range(0, parkingSpots[lot].Count)
                .Select(_ => carPrefabs[Random.Range(0, carPrefabs.Length)])
                .Take(maxOccupiedSpaces)
                .ToList();

            var spots = Enumerable.Range(0, parkingSpots[lot].Count)
                .ToList()
                .Shuffle()
                .Take(maxOccupiedSpaces)
                .Select(i => parkingSpots[lot][i])
                .ToList();

            // occu
            foreach(var spot in spots)
            {
                foreach (Transform child in spot.transform)
                {
                    if(child.name == ParkingUtils.Marker)
                    {
                        child.gameObject.tag = ParkingUtils.CarTag;
                        occupiedSpots.Add(child.gameObject);
                        break;
                    }
                }
            }

            // rotation angles for y axis: 90 or -90
            var angles = Enumerable.Range(0, maxOccupiedSpaces)
                .Select(_ => 90 * (Random.Range(0, 2) * 2 - 1))
                .ToList();

           for(int i = 0; i < maxOccupiedSpaces; i++)
           {
                GameObject car = CloneCar(prefabs[i],
                    spots[i].transform.position,
                    Quaternion.AngleAxis(angles[i], transform.up));
                instantiatedCars.Add(car);
           }
        }
    }

    private void DiscoverFreeSpots()
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

    /// <summary>
    /// Is the position in a free spot
    /// </summary>
    /// <param name="pos"></param>
    public bool IsFreeSpot(Vector3 pos)
    {
        var freeSpots = FreeSpots
            .Where(go => go.transform.Find("Marker").GetComponent<BoxCollider>().bounds.Contains(pos))
            .Count();
        return freeSpots > 0;
    }

    void DestroyCars()
    {
        if(instantiatedCars?.Count == 0)
        {
            return;
        }

        foreach (var car in instantiatedCars)
        {
            // Destroy() will wait unitl next Update()
            // which will cause newly generated cars to 
            // collide with the ones awaiting destruction
            DestroyImmediate(car);
        }

        foreach (var cube in parkingBlockers)
        {
            DestroyImmediate(cube);
        }

        foreach (var spot in occupiedSpots)
        {
            spot.tag = ParkingUtils.ParkingTag; 
        }

        instantiatedCars.Clear();
        parkingBlockers.Clear();
        parkingSpots.Clear();
        occupiedSpots.Clear();
    }
}
