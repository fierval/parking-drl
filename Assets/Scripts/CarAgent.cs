using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    SpawnParkedCars carSpawner;
    RayPerception3D rayPerception;

    [SerializeField] 
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rayPerception = GetComponent<RayPerception3D>();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);
    }

    public override void CollectObservations()
    {
        base.CollectObservations();
    }
    public override void AgentReset()
    {
        base.AgentReset();
        carSpawner.Spawn();
    }
}
