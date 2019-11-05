using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESShowForwardDirection : MonoBehaviour {
    [SerializeField]
    private float raylength = 6;
    [SerializeField]
    private Color LineColor = new Vector4(1,1,1,1);
    public bool ResizeCollider;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = LineColor;
    Vector3 dir = transform.TransformDirection(Vector3.forward) * raylength;
    Gizmos.DrawRay(transform.position, dir);
   
    }
}
