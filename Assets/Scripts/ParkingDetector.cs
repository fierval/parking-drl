using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingDetector : MonoBehaviour
{
    GameObject parkingBounds;
    Renderer parkRender;
    Bounds parkingBoundingBox;

    private void Awake()
    {
        parkingBounds = transform.Find("ParkingBounds").gameObject;
        parkRender = parkingBounds.GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        parkingBoundingBox = parkRender.bounds;
    }

    // Update is called once per frame
    void Update()
    {
        parkingBoundingBox = parkRender.bounds;
    }

    // parking barriers are the only one tri
    private void OnTriggerEnter(Collider other)
    {
        
    }
}
