using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ParkingState : int
{
    Available = 0,
    InProgress = 1,
    Incomplete = 2,
    Complete = 4
}

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

        placeMatBounds = CreateRectFromTransformAndLocalBounds(transform, GetComponent<MeshFilter>().mesh.bounds);

        parkingProfile = null;
    }

    static Rect CreateRectFromTransformAndLocalBounds(Transform transform, Bounds localBounds)
    {
        var min = transform.TransformVector(localBounds.min);
        var max = transform.TransformVector(localBounds.max);
        
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
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

        var carBounds = CreateRectFromTransformAndLocalBounds(parkingProfile.transform, parkingProfile.GetComponent<MeshFilter>().mesh.bounds);

        if(IsCarInside(placeMatBounds, carBounds))
        {
            SetState(ParkingState.Complete);
        }
    }

    public static bool IsCarInside(Rect parkingBounds, Rect carBounds)
    {
        return parkingBounds.min.x <= carBounds.min.x
            && parkingBounds.min.y <= carBounds.min.y
            && parkingBounds.max.x >= carBounds.max.y
            && parkingBounds.max.y >= carBounds.max.y;
    }

    private void OnCollisionExit(Collision collision)
    {
        numCars--;
        if (numCars > 0) { return; }

        SetState(ParkingState.Available);
    }
}
