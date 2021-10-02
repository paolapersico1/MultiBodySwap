using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Test : MonoBehaviour
{

    public Transform hand;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = hand.TransformPoint(offset);
    }
}
