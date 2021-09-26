using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Test : MonoBehaviour
{

    public Transform hand;
    public Transform head;

    public float zDistance;

    // Start is called before the first frame update
    void Start()
    {
        zDistance = Vector3.Distance(hand.position, head.position);
    }

    // Update is called once per frame
    void Update()
    {
        zDistance = Vector3.Distance(hand.position, head.position);
    }
}
