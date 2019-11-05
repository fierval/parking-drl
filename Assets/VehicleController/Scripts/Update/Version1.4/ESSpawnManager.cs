using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class ESSpawnManager : MonoBehaviour {
    [HideInInspector]
    public bool _destroy = false;
    [HideInInspector]
    public bool triggered;
    [HideInInspector]
    public float mytime = 0;

    public string GameObjectName;
    public float _DestroyTime;
    public ESLapManager lapmanager;
    public Vector3 pos;
    public Quaternion rot;

    void Start()
    {
        this.GetComponent<SphereCollider>().isTrigger = true;
        lapmanager = GetComponent<ESLapManager>();
        this.transform.name = GameObjectName;
    }

    void Update()
    {
        CheckForFlipState();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "DeathZone")
        {
            triggered = true;
        }
        if (other.gameObject.tag == "DeathImmidiate")
        {
            _destroy = true;
        }
        if (other.gameObject.tag == "Nodes")
        {
            pos = other.gameObject.transform.position;
            rot = other.gameObject.transform.rotation;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "DeathZone")
        {
            triggered = false;
        }
    }

    void CheckForFlipState()
    { 
        //check for upsidedown
       
        if(Vector3.Dot(transform.up,Vector3.down) > 0)
        {
            mytime += Time.deltaTime;
            if (mytime > 5)
            {
                _destroy = true;
            }
        }
        //check for side ways
        else if (Mathf.Abs(Vector3.Dot(transform.up, Vector3.down)) < 0.125f)
        {
            mytime += Time.deltaTime;
            if (mytime > 5)
            {
                _destroy = true;
            }
         
        }
        else if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.down)) > 0.825f)
        {
            mytime += Time.deltaTime;
            if (mytime > 5)
            {
                _destroy = true;
            }
        }
            //when it collides with a wall nd gets stuck 
        else if (triggered)
        {
            mytime += Time.deltaTime;
            if (mytime > 5)
            {
                _destroy = true;
            }
        }
        else if (lapmanager.ang < 0)
        {
            mytime += Time.deltaTime;
            if (mytime > 5)
            {
                _destroy = true;
            }
        }
        else
        {
            mytime = 0;
        }

    }
}
