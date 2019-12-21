using System.Collections;
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
    ParkingDetector parkingDetector;
    CollisionState collistionState;

    float[] parkingStateVector;

    const float RayDistance = 20f;

    const float AngleEvery = 15;

    // idx's of 90 and -90 in the array of angles
    const int idx90 = 6;
    const int idx270 = 18;

    // backward and forward facing angles
    readonly float[] directionAngles = { 90, 90 - AngleEvery, 90 + AngleEvery, -90, -90 - AngleEvery, -90 + AngleEvery };

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    readonly string[] DetectableObjects = { "car", "immovable", "parking" };
    readonly string[] ParkingDirection = { "parking" };

    float[] rayAngles;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        parkingDetector = GetComponent<ParkingDetector>();
        collistionState = gameObject.GetComponent<CollisionState>();

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

        // are we heading towards parking? Rear or front?
        var parkingDirectionHits = rayPerception.Perceive(RayDistance, directionAngles, ParkingDirection, 0.2f, 0.2f);

        var maxDistance = 
            parkingDirectionHits
                .Where((_, i) => i % 3 == 2)
                .Max();

        // negative reward for not heading towards parking
        if(maxDistance == 0)
        {
            return -1e-3f;
        }

        // small reward for getting closer to parking
        return 1f / maxDistance * 1e-3f;
    }

    public override void CollectObservations()
    {
        // observations
        AddVectorObs(rayPerception.Perceive(RayDistance, rayAngles, DetectableObjects, 0.2f, 0.2f));
        // position
        AddVectorObs(transform.position);
        // direction (rotation)
        AddVectorObs(transform.rotation.y);
        // parking state
        AddVectorObs(ToCategory(parkingDetector.CarParkingState));
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
        var action = new float[3];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        action[2] = Input.GetAxis("Jump");
        return action;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var angle in rayAngles)
        {
            var endpos = transform.TransformDirection(RayPerception3D.PolarToCartesian(RayDistance, angle));
            Gizmos.DrawWireSphere(transform.position + endpos, 0.5f);
        }
    }
}
