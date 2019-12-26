﻿using System.Collections;
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
    CollisionState collistionState;
    float [] rewardAngles;

    float[] parkingStateVector;

    // backward and forward facing angles

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    float[] rayAngles;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        parkingDetector = GetComponent<ParkingDetector>();
        collistionState = gameObject.GetComponent<CollisionState>();

        // initialize vector of parking states
        parkingStateVector = new float[Enum.GetNames(typeof(ParkingState)).Length];
    }

    public override void AgentAction(float[] vectorAction)
    {
        gearShift.GearShift(vectorAction[0]);
        vehicleController.Engine(vectorAction[0], vectorAction[1], vectorAction[2]);

        float reward = CollectRewards();
        AddReward(reward);
        SetDone();
    }

    float CollectRewards()
    {
        // are we parking now
        switch (parkingDetector.CarParkingState)
        {
            case ParkingState.InProgress:
                return 1.25e-3f;
            case ParkingState.Failed:
                return -1f;
            case ParkingState.Complete:
                return 1f;
            default:
                break;
        }

        var observations = sensors
            .Select(s => (s as RayPerceptionSensor).Observations)
            .Take(5 * 3)
            .SelectMany(o => o)
            .ToList();

        float maxDistance = 0f;
        for (int i = 2; i < observations.Count; i+=5)
        {
            // hit parking
            if(observations[i] > 0)
            {
                maxDistance = Mathf.Max(maxDistance, observations[i + 2]);
            }
        }        

        // negative reward for not heading towards parking
        if (maxDistance == 0)
        {
            return -1e-3f;
        }

        // small reward for getting closer to parking
        return 1f / maxDistance * 1e-4f;
    }

    public override void CollectObservations()
    {
        // position
        AddVectorObs(transform.position);
        // direction (rotation)
        AddVectorObs(transform.rotation.y);
        // parking state: one-hot observation
        AddVectorObs((int)parkingDetector.CarParkingState, parkingStateVector.Length);
        // collision
        AddVectorObs(collistionState.IsCollsion);
    }

    private void SetDone()
    {
        if (collistionState.IsCollsion
            || parkingDetector.CarParkingState == ParkingState.Complete
            || parkingDetector.CarParkingState == ParkingState.Failed)
        {
            Done();
        }
    }

    private float[] ToCategory(ParkingState state)
    {
        Array.Clear(parkingStateVector, 0, parkingStateVector.Length);
        parkingStateVector[(int)state] = 1f;
        return parkingStateVector;
    }

    public override void AgentReset()
    {
        collistionState.IsCollsion = false;
        carSpawner.Spawn();
        transform.position = startPosTransform.position;
        transform.rotation = startPosTransform.rotation;
    }

    public override float[] Heuristic()
    {
        // update the sensors
        GenerateSensorData();

        var action = new float[3];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        action[2] = Input.GetAxis("Jump");
        return action;
    }

    static Vector3 PolarToCartesian3D(float radius, float angleDegrees)
    {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

}
