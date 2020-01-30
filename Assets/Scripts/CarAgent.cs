using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using MLAgents.Sensor;
using System;
using static MLAgents.Sensor.RayPerceptionSensor;
using UnityEditor;
using TMPro;

public enum Facing :int
{
    WhoCares = 0,
    Front = 1,
    Back = 2
}

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;

    ESVehicleController vehicleController;
    ESGearShift gearShift;
    ParkingDetector parkingDetector;
    float [] rewardAngles;

    int parkingStateLength;
    int facingLength;

    // backward and forward facing angles

    [Tooltip("Starting position for the agent")]
    public Transform startPosTransform;

    bool isCollision;

    // number of tags we are detecting a ray collision with
    int numberOfTags;
    // index of the parking tag in the sensor detection array
    int idxParkingTag;

    int actionSpaceSize;

    const float MinWorldX = -35f, MinWorldZ = -46.9f, MaxWorldZ = 22.64f, MaxWorldX = 36.32f;
    const float deltaX = MaxWorldX - MinWorldX;
    const float deltaZ = MaxWorldZ - MinWorldZ;

    RayPerceptionSensorComponent3D [] raySensors;
    float [] rayAngles;
    float[] observations;

    DebugDisplayInfo rayDebugInfo = null;

    // angles to display
    List<TextMeshPro> tmeshAngles;

    public TextMeshPro angleDisplay;
    public bool showAngles;

    Facing nowFacing = Facing.WhoCares;

    private void Awake()
    {
        vehicleController = GetComponent<ESVehicleController>();
        gearShift = GetComponent<ESGearShift>();
        parkingDetector = GetComponent<ParkingDetector>();

        // initialize vector of parking states
        parkingStateLength = Enum.GetNames(typeof(ParkingState)).Length;
        facingLength = Enum.GetNames(typeof(Facing)).Length;

        isCollision = false;

        RayPerceptionSensorComponent3D rayPerceptionSensorComponent3D = gameObject.GetComponentsInChildren<RayPerceptionSensorComponent3D>().First();
        numberOfTags = rayPerceptionSensorComponent3D.detectableTags.Count;
        idxParkingTag = rayPerceptionSensorComponent3D.detectableTags.FindIndex(s => s == ParkingUtils.ParkingTag);

        actionSpaceSize = GetComponent<BehaviorParameters>().brainParameters.vectorActionSize[0];

        raySensors = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        RayPerceptionSensorComponent3D sensor = raySensors.First();
        rayAngles = RayPerceptionSensorComponent3D.GetRayAngles(sensor.raysPerDirection, raySensors.First().maxRayDegrees);

        observations = new float[(sensor.detectableTags.Count + 2) * rayAngles.Length];
        rayDebugInfo = new DebugDisplayInfo();

        // info for ray drawing
        if (Application.isEditor)
        {
            tmeshAngles = new List<TextMeshPro>();
        }
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
        if (isCollision) return -1f;

        float reward = -1e-4f;

        // are we parking now
        switch (parkingDetector.CarParkingState)
        {
            case ParkingState.InProgress:
                reward = -reward;
                break;
            case ParkingState.Failed:
                return -1f;
            case ParkingState.Complete:
                return 1f;
            default:
                break;
        }

        if (Application.isEditor && showAngles)
        {
            tmeshAngles.ForEach(o => Destroy(o.gameObject));
            tmeshAngles.Clear();
        }

        var minAngleFacing = FindSensorAngleDistanceAdjustFacing();

        // negative reward for not heading towards parking
        if (minAngleFacing.facing == Facing.WhoCares)
        {
            return reward;
        }

        // small reward for getting closer to parking
        // and also turning towards it
        return reward + Mathf.Abs(Mathf.Cos(minAngleFacing.angle)) * 8e-5f + (1f / minAngleFacing.distance) * 5e-5f;
    }

    private (float angle, float distance, Facing facing) FindSensorAngleDistanceAdjustFacing()
    {
        //angle and distance fraction of hitting rays
        // plus where-facing
        (float angle, float distance, Facing facing) anglesDistances = (360f, 1f, Facing.WhoCares);

        foreach (var sensor in raySensors)
        {
            RayPerceptionSensor.PerceiveStatic(sensor.rayLength, rayAngles, sensor.detectableTags, sensor.startVerticalOffset, sensor.endVerticalOffset,
                sensor.sphereCastRadius, sensor.transform, RayPerceptionSensor.CastType.Cast3D, observations, false, debugInfo: rayDebugInfo);

            for (int i = idxParkingTag; i < observations.Length; i += numberOfTags + 2)
            {
                // hit parking
                if (observations[i] > 0)
                {
                    int idx = (i - idxParkingTag) / (sensor.detectableTags.Count + 2);
                    
                    var angle = rayAngles[idx];
                    var distance = observations[i + 2];


                    // get angle relative to local axis z
                    var zAngleFront = GetSensorRotationAngle(sensor.transform, angle);
                    var zAngleBack = (zAngleFront + 180f) % 360f;

                    // don't care where we are moving, the most advantageos direction is saved
                    var finalAngle = Math.Abs(zAngleBack) <= Math.Abs(zAngleFront) ? zAngleBack : zAngleFront;
                    var facing = Math.Abs(zAngleBack) <= Math.Abs(zAngleFront) ? Facing.Back : Facing.Front;

                    if (Mathf.Abs(finalAngle) < Mathf.Abs(anglesDistances.angle)
                        || (Mathf.Abs(finalAngle) == Mathf.Abs(anglesDistances.angle) && distance < anglesDistances.distance))
                    {
                        anglesDistances = (angle: finalAngle, distance: distance, facing: facing);
                    }

                    if (Application.isEditor && showAngles)
                    {
                        DebugDrawAngleValues(idx, zAngleFront, zAngleBack);
                    }
                }
            }
        }

        // this will influence the new state
        nowFacing = anglesDistances.facing;
        return anglesDistances;
    }

    private void DebugDrawAngleValues(int idx, float zAngleFront, float zAngleBack)
    {
        Vector3 endPositionWorld = GetHitEndWorldPos(idx);

        // draw text
        var gObj = Instantiate(angleDisplay);
        gObj.gameObject.SetActive(true);

        endPositionWorld.y = 3;
        gObj.transform.position = endPositionWorld;
        gObj.transform.eulerAngles = new Vector3(90, 0, 0);
        gObj.SetText($"{zAngleFront:.##}, {zAngleBack:.##}");
        tmeshAngles.Add(gObj);
    }

    private Vector3 GetHitEndWorldPos(int idx)
    {
        var rayInfo = rayDebugInfo.rayInfos[idx];

        var startPositionWorld = rayInfo.worldStart;
        var endPositionWorld = rayInfo.worldEnd;
        var rayDirection = endPositionWorld - startPositionWorld;
        rayDirection *= rayInfo.hitFraction;
        endPositionWorld = startPositionWorld + rayDirection;
        return endPositionWorld;
    }

    float GetSensorRotationAngle(Transform sensor, float sensorAngle)
    {
        var localDirection = GetDirectionFromAngle(sensorAngle);
        var worldDirection = sensor.TransformDirection(localDirection);
        var carRelativeDirection = transform.InverseTransformDirection(worldDirection);
        return GetZAngleFromDirection(carRelativeDirection);
    }

    public override void CollectObservations()
    {
        // position
        AddVectorObs(new Vector2((transform.position.x - MinWorldX) / deltaX, (transform.position.z - MinWorldZ) / deltaZ));
        // rotation
        AddVectorObs(transform.eulerAngles.y / 360f);
        // parking state: one-hot observation
        AddVectorObs((int)parkingDetector.CarParkingState, parkingStateLength);
        //facing
        AddVectorObs((int)nowFacing, facingLength);
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

    static Vector3 GetDirectionFromAngle(float angleDegrees)
    {
        var x = Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

    static float GetZAngleFromDirection(Vector3 direction)
    {
        var absAngle = Mathf.Rad2Deg * Mathf.Acos(direction.z);
        return direction.x > 0 ? absAngle : -absAngle;
    }
}
