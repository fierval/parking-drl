using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParkingTracker : MonoBehaviour
{
    bool hasVehicle = false;
    Renderer placeMatRenderer;
    Rect placeMatBounds;
    
    // car ParkingBounds object
    GameObject parkingProfile;

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
                parkingProfile = null;
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
        SetState(ParkingState.Available);
        placeMatRenderer = GetComponent<Renderer>();

        placeMatBounds = ParkingUtils.CreateRectFromTransformAndLocalBounds(transform);

        parkingProfile = null;
    }


    // Parking detector should instantiate proper entry
    // Any entrance by any carBounds means is considered "collision"
    private void OnCollisionEnter(Collision collision)
    {
        if (parkingState == ParkingState.Available)
        {
            SetState(ParkingState.Incomplete);
        }
        numCars++;
    }

    private void OnCollisionStay(Collision collision)
    {
        // we use a different entry mechanism
        Debug.Assert(parkingState != ParkingState.Available);

        if (parkingState == ParkingState.Incomplete) { return; }

        CheckAndSetIsDone(collision.gameObject);
    }

    private void CheckAndSetIsDone(GameObject gameObject)
    {
        if (parkingProfile is null)
        {
            // car -> Wheels -> collider
            var car = gameObject.transform.parent.parent;
            parkingProfile = car.Find("ParkingBounds").gameObject;
        }

        var carBounds = ParkingUtils.CreateRectFromTransformAndLocalBounds(parkingProfile.transform);

        if(placeMatBounds.Contains(carBounds))
        {
            SetState(ParkingState.Complete);
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        numCars--;
        if (numCars > 0) { return; }

        SetState(ParkingState.Available);
    }
}
