using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ParkingTracker : MonoBehaviour
{
    bool hasVehicle = false;
    Renderer placeMatRenderer;
    string spotName;

    ParkingState parkingState;
    // number of cars currently inside the spot
    int numCars = 0;

    public Material emptyMaterial;
    public Material inProgressMaterial;
    public Material completeMaterial;
    public Material incompleteMaterial;

    Material CurMaterial
    {
        get => placeMatRenderer.material;
        set
        {
            placeMatRenderer.material = value;
        }
    }

    void SetState(ParkingState state)
    {
        switch (state)
        {
            case ParkingState.Available:
                CurMaterial = emptyMaterial;
                break;
            case ParkingState.InProgress:
                CurMaterial = inProgressMaterial;
                break;
            case ParkingState.Incomplete:
                CurMaterial = incompleteMaterial;
                break;
            case ParkingState.Complete:
                CurMaterial = completeMaterial;
                break;
            default:
                Debug.Assert(false, "Unknown state");
                break;
        }
        parkingState = state;
    }

    private void Awake()
    {
        placeMatRenderer = GetComponent<Renderer>();
        SetState(ParkingState.Available);
        spotName = transform.parent.name;

        ParkingDetector.Parked += OnParked;
        ParkingDetector.Parking += OnParking;
        ParkingDetector.ExitedParking += OnParkingExit;
    }

    void OnParked(string [] spots)
    {
        if(spots.First() != spotName || ParkingState.Complete == parkingState) { return; }

        SetState(ParkingState.Complete);
    }

    void OnParking(string [] spots)
    {
        if(!spots.Contains(spotName) || ParkingState.InProgress == parkingState) { return; }

        SetState(ParkingState.InProgress);
    }

    void OnParkingExit(string [] spots)
    {
        if (!spots.Contains(spotName) || ParkingState.Available == parkingState) { return; }

        SetState(ParkingState.Available);

    }
}

