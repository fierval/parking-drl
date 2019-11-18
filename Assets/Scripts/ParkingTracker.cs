using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingTracker : MonoBehaviour
{
    bool hasVehicle = false;
    Renderer placeMatRenderer;
    Material curMaterial;

    // number of cars currently inside the spot
    int numCars = 0;

    public Material emptyMaterial;
    public Material inProgressMaterial;
    public Material completeMaterial;
    public Material incompleteMaterial;

    Material CurMaterial
    {
        get => curMaterial;
        set
        { curMaterial = value; placeMatRenderer.material = curMaterial; }
    }

    private void Awake()
    {
        curMaterial = emptyMaterial;
        placeMatRenderer = GetComponent<Renderer>();
    }

    // Parking detector should instantiate proper entry
    // Any entrance by any other means is considered "collision"
    private void OnCollisionEnter(Collision collision)
    {
        if(IsAvailable)
        {
            CurMaterial = incompleteMaterial;
        }
        numCars++;
    }

    private void OnCollisionStay(Collision collision)
    {
        // we use a different entry mechanism
        Debug.Assert(!IsAvailable);

        if(IsCollidsion) { return; }
        bool done = IsDone(collision.gameObject);
        if (!done) { return; }
        CurMaterial = completeMaterial;        
    }

    private bool IsDone(GameObject gameObject)
    {
        throw new NotImplementedException();
    }

    private void OnCollisionExit(Collision collision)
    {
        numCars--;
        if(numCars > 0) { return; }

        CurMaterial = emptyMaterial;
    }

    bool IsAvailable => CurMaterial == emptyMaterial;
    bool IsParking => CurMaterial == inProgressMaterial;
    bool IsCollidsion => CurMaterial == incompleteMaterial;
    bool IsSuccess => CurMaterial == completeMaterial;
   
}
