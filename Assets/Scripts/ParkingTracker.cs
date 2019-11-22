using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ParkingTracker : MonoBehaviour
{
    Renderer placeMatRenderer;
    GameObject spot;

    ParkingState parkingState;

    public Material emptyMaterial;
    public Material inProgressMaterial;
    public Material completeMaterial;
    public Material failedMaterial;

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
            case ParkingState.Failed:
                CurMaterial = failedMaterial;
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

        spot = transform.parent.gameObject;

        ParkingDetector.Parked += OnParked;
        ParkingDetector.Parking += OnParking;
        ParkingDetector.ExitedParking += OnParkingExit;
        ParkingDetector.ParkingFailed += OnParkingFailed;
    }

    public ParkingState ParkingState
    {
        set
        {
            if (value == parkingState) { return; }
            SetState(value);

        }
    }
}

