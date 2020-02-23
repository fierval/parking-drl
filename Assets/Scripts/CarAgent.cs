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

/// <summary>
/// Reward structure for the agent
/// </summary>
struct Rewards
{
    public const float BaseReward = -1e-3f;
    public const float DistanceWeight = 1e-4f;
    // should line up with the parking spot
    public const float AngleWeight = 1e-4f;
    // should be as close to 0 as possible when parking
    // so we punish for having velocity
    public const float VelocityWeight = -1e-4f;
    public const float ParkingComplete = 1e-2f;
    public const float Success = 1f;
    public const float ParkingFailed = -1f;
    public const float ParkingProgress = 1e-2f;
    public const float FoundParking = -BaseReward;

    // how many steps left before we totally fail
    public const int StepsToFailure = 1;
}

public class CarAgent : Agent
{
    public SpawnParkedCars carSpawner;

    ESVehicleController vehicleController;
    ESGearShift gearShift;
    ParkingDetector parkingDetector;

    // to interface with curriculum learning
    CarAcademy academy;

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

    // where we expect to park
    GameObject goalParking;
    private Vector2 goalRelativeParkingPosition;

    // place mat of the free spot
    Transform placeMat;
    GameObject freeSpace;
    Vector2 goalParkingPosition;

    private void Awake()
    {
        academy = FindObjectOfType<CarAcademy>();

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

        freeSpace = null;

        // info for ray drawing
        if (Application.isEditor)
        {
            tmeshAngles = new List<TextMeshPro>();
        }

        Monitor.SetActive(true);
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
        Monitor.Log("CumReward", GetCumulativeReward().ToString(), transform);
        SetDone();
    }

    private float Clamp(float v)
    {
        return Mathf.Clamp(v, -1f, 1f);
    }

    float CollectRewards()
    {
        if(IsDone()) { return 0; }

        float reward = Rewards.BaseReward;

        if (isCollision) return Rewards.ParkingFailed;

        Vector3 velocity = Vector3.zero;
        var angle = Mathf.Deg2Rad * parkingDetector.GetForwardParkingAngle(placeMat.gameObject);

        // we add rewards for parking in the goal spot
        bool isParking = parkingDetector.IsParkingInThisSpot(placeMat.gameObject);
        if (isParking)
        {
            // are we parking now
            switch (parkingDetector.CarParkingState)
            {
                case ParkingState.InProgress:
                    reward = Rewards.ParkingProgress;
                    break;
                case ParkingState.Failed:
                    return Rewards.ParkingFailed;
                case ParkingState.Complete:
                    reward = Rewards.ParkingComplete;
                    velocity = vehicleController.CarRb.velocity;
                    if(velocity.magnitude <= ParkingUtils.TerminalVelocity)
                    {
                        reward += Rewards.Success;
                        return reward;
                    }
                    break;
                default:
                    break;
            }
        }

        int NoDistanceIfCompleteParking() => isParking && parkingDetector.CarParkingState == ParkingState.Complete ? 0 : 1;

        if (Application.isEditor && showAngles)
        {
            tmeshAngles.ForEach(o => Destroy(o.gameObject));
            tmeshAngles.Clear();
        }

        var distance = GetDistanceFromGoal();
        var rewardDistance = (1f / (distance + 1e-7f)) * (1.4f - Mathf.Pow(distance, 1.1f));

        // small reward for getting closer to parking
        // and also turning towards it
        reward +=
            NoDistanceIfCompleteParking() * rewardDistance * Rewards.DistanceWeight
            + Mathf.Abs(Mathf.Cos(angle)) * Rewards.AngleWeight
            + velocity.magnitude * Rewards.VelocityWeight;

        Monitor.Log("Distance", distance.ToString(), transform);
        Monitor.Log("RewardDistance", rewardDistance, transform);
        Monitor.Log("CosAngle", Mathf.Cos(angle), transform);
        Monitor.Log("Reward", reward.ToString(), transform);
        return reward;
    }

    float GetRelativeDistanceFromGoal() => 
        Vector2.Distance(goalRelativeParkingPosition, GetNormalizedPosition());

    float GetDistanceFromGoal() => Vector2.Distance(goalParkingPosition, new Vector2(transform.position.x, transform.position.z));

    Vector2 GetNormalizedPosition() => NormalizePos(new Vector2(transform.position.x, transform.position.z));

    Vector2 NormalizePos(Vector2 pos) => new Vector2((pos.x - MinWorldX) / deltaX, (pos.y - MinWorldZ) / deltaZ);

    Vector2 GetPositionRelativeToGoal()
    {
        var relativePos = goalParking.transform.InverseTransformVector(transform.position);
        return NormalizePos(new Vector2(relativePos.x, relativePos.z));
    }

    private (float angle, float distance, Facing facing) FindSensorAngleDistanceAdjustFacing()
    {
        //angle and distance fraction of hitting rays
        // plus where-facing
        (float angle, float distance, Facing facing) anglesDistances = (360f, 1f, Facing.WhoCares);

        foreach (var sensor in raySensors)
        {
            observations = GetSensorObservations(sensor);

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
                    var zAngleBack = (zAngleFront + 180f);

                    if(zAngleBack > 180)
                    {
                        zAngleBack -= 360;
                    }

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

    private float [] GetSensorObservations(RayPerceptionSensorComponent3D sensor)
    {
        RayPerceptionSensor.PerceiveStatic(sensor.rayLength, rayAngles, sensor.detectableTags, sensor.startVerticalOffset, sensor.endVerticalOffset,
            sensor.sphereCastRadius, sensor.transform, RayPerceptionSensor.CastType.Cast3D, observations, false, debugInfo: rayDebugInfo);

        return observations;
    }

    /// <summary>
    /// We have hit a free parking spot. Return its transform so we can track our progress from that moment on
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="idx"></param>
    /// <returns></returns>
    private GameObject GetFreeParkingSpot(RayPerceptionSensorComponent3D sensor, int idx)
    {
        var castRadius = sensor.sphereCastRadius;

        var rayInfo = rayDebugInfo.rayInfos[idx];
        bool castHit = false;
        var rayLength = sensor.rayLength;
        int layerMask = Physics.DefaultRaycastLayers;
        var startPositionWorld = rayInfo.worldStart;
        var rayDirection = rayInfo.worldEnd - startPositionWorld;
        RaycastHit rayHit;

        if (castRadius > 0f)
        {
            castHit = Physics.SphereCast(startPositionWorld, castRadius, rayDirection, out rayHit,
                rayLength, layerMask);
        }
        else
        {
            castHit = Physics.Raycast(startPositionWorld, rayDirection, out rayHit,
                rayLength, layerMask);
        }

        var parkingObject = castHit ? rayHit.collider.gameObject.transform.parent.gameObject : null;
        return parkingObject;
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

    /// <summary>
    /// Where is the ray going to end
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Given a sensor and its angle, find the angle
    /// relative to the car forward axis
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="sensorAngle"></param>
    /// <returns></returns>
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
        AddVectorObs(GetPositionRelativeToGoal());

        // velocity
        var velocity = vehicleController.CarRb == null ? Vector3.zero : vehicleController.CarRb.velocity;
        AddVectorObs(new Vector2(velocity.x, velocity.z));
        
        // parking state: one-hot observation
        if (parkingDetector.IsParkingInThisSpot(placeMat.gameObject))
        {
            AddVectorObs((int)parkingDetector.CarParkingState, parkingStateLength);
            AddVectorObs(Mathf.Cos(parkingDetector.GetForwardParkingAngle(placeMat.gameObject)));
        }
        else
        {
            AddVectorObs(0, parkingStateLength);
            AddVectorObs(0);
        }

        //relative rotation
        AddVectorObs(parkingDetector.GetForwardRotation(placeMat));

        // collision
        AddVectorObs(isCollision);
    }

    private void SetDone()
    {
        var isParking = parkingDetector.IsParkingInThisSpot(placeMat.gameObject);
        if (isCollision
            || (isParking && parkingDetector.CarParkingState == ParkingState.Complete && vehicleController.CarRb.velocity.magnitude <= ParkingUtils.TerminalVelocity)
            || (isParking && parkingDetector.CarParkingState == ParkingState.Failed))
        {
            Monitor.print($"IsCollision: {isCollision}, Reward: {GetCumulativeReward()}," +
                $" Parked: {parkingDetector.CarParkingState == ParkingState.Complete && vehicleController.CarRb.velocity.magnitude <= 2}");
            Done();
        }
    }

    public override void AgentReset()
    {
        goalParking = null;
        isCollision = false;

        // set academy properties based on the curriculum
        carSpawner.randomGoalSpot = academy.FloatProperties.GetPropertyWithDefault(ParkingUtils.RandomSpot, 1f) > 0;
        carSpawner.goalSpot = (int) academy.FloatProperties.GetPropertyWithDefault(ParkingUtils.GoalSpot, 3f);
        carSpawner.maxOccupiedSpaces = (int)academy.FloatProperties.GetPropertyWithDefault(ParkingUtils.NumCars, 4f);

        carSpawner.Spawn();
        goalParking = carSpawner.GoalParkingSpot();
        goalRelativeParkingPosition = new Vector2((goalParking.transform.position.x - MinWorldX) / deltaX, (goalParking.transform.position.z - MinWorldZ) / deltaZ);
        goalParkingPosition = new Vector2(goalParking.transform.position.x, goalParking.transform.position.z);

        // place the target rectangle
        DestroyImmediate(freeSpace);
        placeMat = GetPlaceMat(goalParking);

        freeSpace = Instantiate(academy.FreeSpotMarker, placeMat.transform.position, placeMat.transform.rotation, placeMat);

        transform.position = startPosTransform.position;
        transform.rotation = startPosTransform.rotation;
        
        if(vehicleController.CarRb != null)
        {
            vehicleController.CarRb.velocity = Vector3.zero;
        }
    }

    private Transform GetPlaceMat(GameObject goalParking)
    {
        Transform placeMat = null;
        foreach (Transform transform in goalParking.transform)
        {
            if(transform.name == ParkingUtils.PlaceMat)
            {
                placeMat = transform;
                break;
            }
        }

        return placeMat;
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
