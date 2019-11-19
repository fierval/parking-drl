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
    public delegate void parked(string name);
    public event parked Parked;

    Renderer parkRender;
    Bounds parkingBoundingBox;
    ESVehicleController vehicleController;

    WheelCollider[] wheelColliders;
    HashSet<(int, string)> parkingState;

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
        parkingState.Clear();

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wheel = wheelColliders[i];
            WheelHit hit;
            wheel.GetGroundHit(out hit);
            if (hit.collider != null && hit.collider.gameObject.name == Consts.PlaceMat)
            {
                parkingState.Add((i, hit.collider.gameObject.transform.parent.name));

            }
        }

        if (IsParked)
        {
            Parked?.Invoke(parkingState.AsEnumerable().First().Item2);
        }
    }

    // We only have as many points as we have colliders and they
    // are all within one placemat
    bool IsParked 
    {
        get
        {
            return parkingState.Count == wheelColliders.Length
               && parkingState.Select((i, name) => name).Distinct().Count() == 1;
        }
    }

    // We have a wheel on the mat or a few wheels
    // on a couple of mats
    bool IsParking
    {
        get 
        {
            return parkingState.Count > 0
                && (parkingState.Count != wheelColliders.Length || parkingState.Select((i, name) => name).Distinct().Count() != 1);
        }
    }
}
