using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayShooter : MonoBehaviour
{
    private GameObject plane;

    // Start is called before the first frame update
    private void Awake()
    {
        plane = GameObject.FindGameObjectWithTag("cube");
    }
    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, plane.transform.position, Color.blue);
        RaycastHit hit;
        var pos = transform.position;
        var to = Quaternion.AngleAxis(-45, transform.TransformDirection(Vector3.right)) * transform.TransformDirection(Vector3.forward);
        //var to = transform.TransformDirection(Vector3.forward);
        Ray ray = new Ray(pos, to);
        Debug.DrawRay(pos, to, Color.red);

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("plane")))
        {
            Debug.DrawRay(pos, to, Color.green);
        }
    }
}
