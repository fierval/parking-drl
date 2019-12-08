using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;
    RayPerception3D rayPerception;

    const float RayDistance = 20f;
    const int AngleEvery = 10;
    readonly string[] DetectableObjects = {"car", "immovable", "parking"};
    readonly float[] rayAngles;

    public CarAgent()
    {
        rayAngles = Enumerable.Range(0, 360)
            .Where(i => i % AngleEvery == 0)
            .Select(i => (float) i)
            .ToArray();
    }

    [SerializeField] 
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rayPerception = GetComponent<RayPerception3D>();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
    }

    public override void CollectObservations()
    {
        AddVectorObs(rayPerception.Perceive(RayDistance, rayAngles, DetectableObjects, 0f, 0f));
        AddVectorObs(transform.position);
        AddVectorObs(transform.rotation.y);
    }

    public override void AgentReset()
    {
        base.AgentReset();
        carSpawner.Spawn();
    }
}
