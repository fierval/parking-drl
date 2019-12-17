using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionState : MonoBehaviour
{
    public bool IsCollsion { get; private set; }

    private void Awake()
    {
        IsCollsion = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject != null)
        {
            IsCollsion = true;
        }
    }
}
