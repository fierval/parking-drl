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
    // fires when a vehicle is parked
    // to notify a placemat
    public delegate void ParkingAction(string [] name);
    
    public static event ParkingAction Parked;
    public static event ParkingAction Parking;
    public static event ParkingAction ExitedParking;
    
    Renderer parkRender;
    Bounds parkingBoundingBox;
    ESVehicleController vehicleController;

    WheelCollider[] wheelColliders;
    Dictionary<string, int> parkingState = new Dictionary<string, int>();

    bool isParked = false;

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
        HashSet<GameObject> placeMats = new HashSet<GameObject>();

        //TODO: Save current parking state so we can check if it's time to reset
        // parking spaces
        Dictionary<string, int> curParkingState = new System.Collections.Generic.Dictionary<string, int>();

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wheel = wheelColliders[i];
            WheelHit hit;
            wheel.GetGroundHit(out hit);
            if(hit.collider == null || hit.collider.gameObject == null) { continue; }

            string hitName = hit.collider.gameObject.name;
            if (hit.collider != null && hitName == Consts.PlaceMat)
            {
                if (!parkingState.ContainsKey(hitName))
                {
                    parkingState.Add(hitName, 0);
                }
                parkingState[hitName]++;
            }

        }

        if (IsParked(curParkingState))
        {
            Parked?.Invoke(parkingState.Keys.ToArray());
            return;
        }

        if(IsParking(curParkingState))
        {
            Parking?.Invoke(parkingState.Keys.ToArray());
            return;
        }

        var exitedParkingSpaces = NoLongerParking(curParkingState.Keys, parkingState.Keys);
        if(exitedParkingSpaces.Length > 0)
        {
            ExitedParking?.Invoke(exitedParkingSpaces);
        }

        parkingState = curParkingState.Select(kvp => kvp).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    // We only have as many points as we have colliders and they
    // are all within one placemat
    bool IsParked(Dictionary<string, int> parkingState) => parkingState.Count == 1 && parkingState.First().Value == wheelColliders.Length;

    // We have a wheel on the mat or a few wheels
    // on a couple of mats
    bool IsParking(Dictionary<string, int> parkingState) => parkingState.FirstOrDefault().Value > 0;

    string [] NoLongerParking(IEnumerable<string> curParking, IEnumerable<string> prevParking)
    {
        return prevParking.Except(curParking).ToArray();
    }
}
