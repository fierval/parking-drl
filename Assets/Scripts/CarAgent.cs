﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using System;

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;

    RayPerception3D rayPerception;
    ESVehicleController vehicleController;
    ESGearShift gearShift;
    Transform carTransform;
    ParkingDetector parkingDetector;
    float[] parkingStateVector;

    const float RayDistance = 20f;

    const float AngleEvery = 15;

    // idx's of 90 and -90 in the array of angles
    const int idx90 = 6;
    const int idx270 = 18;

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    readonly string[] DetectableObjects = {"car", "immovable", "parking"};
    float[] rayAngles;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        carTransform = GetComponent<Transform>();
        parkingDetector = GetComponent<ParkingDetector>();

        // initialize vector of parking states
        parkingStateVector = new float[Enum.GetNames(typeof(ParkingState)).Length];
        Array.Clear(parkingStateVector, 0, parkingStateVector.Length);

        rayAngles = Enumerable.Range(0, 360)
            .Where(i => i % AngleEvery == 0)
            .Select(i => (float)i)
            .ToArray();
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rayPerception = GetComponent<RayPerception3D>();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        gearShift.GearShift(vectorAction[0]);
        vehicleController.Engine(vectorAction[0], vectorAction[1], vectorAction[2]);

        CollectRewards();
    }

    private void CollectRewards()
    {
        
    }

    public override void CollectObservations()
    {
        AddVectorObs(rayPerception.Perceive(RayDistance, rayAngles, DetectableObjects, 0.2f, 0.2f));
        AddVectorObs(transform.position);
        AddVectorObs(transform.rotation.y);
        AddVectorObs(ToCategory(parkingDetector.CarParkingState));
    }

    private float[] ToCategory(ParkingState state)
    {
        Array.Clear(parkingStateVector, 0, parkingStateVector.Length);
        parkingStateVector[(int)Mathf.Log((float)state, 2f)] = 1f;
        return parkingStateVector;
    }

    public override void AgentReset()
    {
        carSpawner.Spawn();
        carTransform.position = startPosTransform.position;
        carTransform.rotation = startPosTransform.rotation;
    }

    public override float[] Heuristic()
    {
        var action = new float[3];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        action[2] = Input.GetAxis("Jump");
        return action;
    }

}
