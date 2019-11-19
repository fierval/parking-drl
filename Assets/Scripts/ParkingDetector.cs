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
    GameObject parkingBounds;
    Renderer parkRender;
    Bounds parkingBoundingBox;
    ESVehicleController vehicleController;

    WheelCollider[] wheelColliders;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        
        // get front and rear wheel colliders
        wheelColliders =
            vehicleController.m_wheelsettings.frontwheels.frontwheelcols.Concat(
                vehicleController.m_wheelsettings.rearwheels.rearwheelcols
                ).ToArray();

        parkingBounds = transform.Find("ParkingBounds").gameObject;
        parkRender = parkingBounds.GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        parkingBoundingBox = parkRender.bounds;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wheel = (WheelCollider)wheelColliders[i];
            WheelHit hit;
            wheel.GetGroundHit(out hit);
            if (hit.collider != null)
            {
                Debug.Log($"Wheel {((WheelColliderPos)i)} hit {hit.collider.gameObject.name}");
            }
        }
    }

}
