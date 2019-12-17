using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;

    RayPerception3D rayPerception;
    ESVehicleController vehicleController;
    ESGearShift gearShift;
    Transform carTransform;

    const float RayDistance = 20f;

    [Range(10, 90), Tooltip("Angle of fanned out raycast")]
    public float angleEvery = 15;

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    readonly string[] DetectableObjects = {"car", "immovable", "parking"};
    float[] rayAngles;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        carTransform = GetComponent<Transform>();

        rayAngles = Enumerable.Range(0, 360)
            .Where(i => i % angleEvery == 0)
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
    }

    public override void CollectObservations()
    {
        AddVectorObs(rayPerception.Perceive(RayDistance, rayAngles, DetectableObjects, 0.2f, 0.2f));
        AddVectorObs(transform.position);
        AddVectorObs(transform.rotation.y);
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
