using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WheelColliderPos : int
{
    FrontRight = 0,
    FrontLeft = 1,
    RearRight = 2,
    RearLeft = 3
}

public class ParkingDetector : MonoBehaviour
{
    Renderer parkRender;
    Bounds parkingBoundingBox;
    ESVehicleController vehicleController;

    WheelCollider[] wheelColliders;
    Dictionary<GameObject, int> parkingState = new Dictionary<GameObject, int>();

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();

        // get front and rear wheel colliders
        wheelColliders =
            vehicleController.m_wheelsettings.frontwheels.frontwheelcols.Concat(
                vehicleController.m_wheelsettings.rearwheels.rearwheelcols
                ).ToArray();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Dictionary<GameObject, int> curParkingState = new Dictionary<GameObject, int>();

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wheel = wheelColliders[i];
            WheelHit hit;
            wheel.GetGroundHit(out hit);
            if(hit.collider == null || hit.collider.gameObject == null) { continue; }

            GameObject spotGameObj = hit.collider.gameObject;
            string hitName = spotGameObj.name;

            // we are in a parking spot
            if (hitName == Consts.PlaceMat)
            {
                if (!curParkingState.ContainsKey(spotGameObj))
                {
                    curParkingState.Add(spotGameObj, 0);
                }

                curParkingState[spotGameObj]++;
            }

        }

        // first we figure out the exited spots & save state
        var exitedParkingSpaces = NoLongerParking(curParkingState.Keys, parkingState.Keys);

        if (exitedParkingSpaces.Length > 0)
        {
            SetParkingState(exitedParkingSpaces, ParkingState.Available);
        }

        parkingState = curParkingState.Select(kvp => kvp).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var parkingObjects = curParkingState.Keys.ToArray();

        // Check if we are parked
        if (IsParked(curParkingState))
        {
            SetParkingState(parkingObjects, ParkingState.Complete);
        }
        else if (IsDoubleParked(curParkingState))
        {
            SetParkingState(parkingObjects, ParkingState.Failed);
        }
        // trying to park
        else if (IsParking(curParkingState))
        {
            SetParkingState(parkingObjects, ParkingState.InProgress);
        }

    }

    void SetParkingState(GameObject [] spots, ParkingState state)
    {
        spots.ToList().ForEach(g => g.GetComponent<ParkingTracker>().ParkingState = state);
    }

    // We only have as many points as we have colliders and they
    // are all within one placemat
    bool IsParked(Dictionary<GameObject, int> parkingState) => parkingState.Count == 1 && parkingState.First().Value == wheelColliders.Length;

    // We have a wheel on the mat or a few wheels
    // on a couple of mats
    bool IsParking(Dictionary<GameObject, int> parkingState) => parkingState.FirstOrDefault().Value > 0;

    bool IsDoubleParked(Dictionary<GameObject, int> parkingState) => parkingState.Count > 1;

    GameObject [] NoLongerParking(IEnumerable<GameObject> curParking, IEnumerable<GameObject> prevParking)
    {
        return prevParking.Except(curParking).ToArray();
    }
}
