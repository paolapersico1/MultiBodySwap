using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public bool hasCollided;

    private void Start()
    {
        hasCollided = false;
    }

    void OnTriggerEnter(Collider other)
    {
        hasCollided = true;
    }

    void OnTriggerExit(Collider other)
    {
        hasCollided = false;
    }

    public bool HasCollided()
    {
        return hasCollided;
    }
}
