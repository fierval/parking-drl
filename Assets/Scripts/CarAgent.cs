using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using MLAgents.Sensor;
using System;

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;

    ESVehicleController vehicleController;
    ESGearShift gearShift;
    ParkingDetector parkingDetector;
    float [] rewardAngles;

    int parkingStateLength;

    // backward and forward facing angles

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    float[] rayAngles;
    bool isCollision;

    // number of tags we are detecting a ray collision with
    int numberOfTags;
    // index of the parking tag in the sensor detection array
    int idxParkingTag;

    int actionSpaceSize;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        parkingDetector = GetComponent<ParkingDetector>();

        // initialize vector of parking states
        parkingStateLength = Enum.GetNames(typeof(ParkingState)).Length;
        isCollision = false;

        RayPerceptionSensorComponent3D rayPerceptionSensorComponent3D = gameObject.GetComponentsInChildren<RayPerceptionSensorComponent3D>().First();
        numberOfTags = rayPerceptionSensorComponent3D.detectableTags.Count;
        idxParkingTag = rayPerceptionSensorComponent3D.detectableTags.FindIndex(s => s == ParkingUtils.ParkingTag);

        actionSpaceSize = GetComponent<BehaviorParameters>().brainParameters.vectorActionSize[0];
    }

    private void OnCollisionEnter(Collision collision)
    {
        isCollision = collision.gameObject != null;
    }

    public override void AgentAction(float[] vectorAction)
    {
        gearShift.GearShift(Clamp(vectorAction[0]));
        vehicleController.Engine(Clamp(vectorAction[0]), Clamp(vectorAction[1]), 0f);

        float reward = CollectRewards();
        AddReward(reward);
        SetDone();
    }

    private float Clamp(float v)
    {
        return Mathf.Clamp(v, -1f, 1f);
    }


    float CollectRewards()
    {
        // are we parking now
        switch (parkingDetector.CarParkingState)
        {
            case ParkingState.InProgress:
                return 1.5e-4f;
            case ParkingState.Failed:
                return -1f;
            case ParkingState.Complete:
                return 1f;
            default:
                break;
        }

        var observations = sensors
            .Take(2) // just vector sensors
            .SelectMany(s => (s as RayPerceptionSensor).Observations)
            .ToList();

        float maxDistance = 0;
        int nParkingHits = 0;
        for (int i = idxParkingTag; i < observations.Count; i += numberOfTags + 2)
        {
            // hit parking
            if(observations[i] > 0)
            {
                maxDistance = Mathf.Max(maxDistance, observations[i + 2]);
                nParkingHits++;
            }
        }

        // negative reward for not heading towards parking
        if (maxDistance == 0)
        {
            return -1e-4f;
        }

        // small reward for getting closer to parking
        // and also turning towards it
        return 1f / maxDistance * nParkingHits * 1e-4f;
    }

    public override void CollectObservations()
    {
        // parking state: one-hot observation
        AddVectorObs((int)parkingDetector.CarParkingState, parkingStateLength);
        // collision
        AddVectorObs(isCollision);
    }

    private void SetDone()
    {
        if (isCollision
            || parkingDetector.CarParkingState == ParkingState.Complete
            || parkingDetector.CarParkingState == ParkingState.Failed)
        {
            Done();
        }
    }

    public override void AgentReset()
    {
        isCollision = false;
        carSpawner.Spawn();
        transform.position = startPosTransform.position;
        transform.rotation = startPosTransform.rotation;
    }

    public override float[] Heuristic()
    {
        // update the sensors
        GenerateSensorData();

        var action = new float[actionSpaceSize];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        return action;
    }

    static Vector3 PolarToCartesian3D(float radius, float angleDegrees)
    {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

}
