using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class PlaneHeight : MonoBehaviour
{
    public float playerHeight = 0;
    public float avatarHeight = 1.7f;

    public void CalibrateHeight(float avatarHeight)
    {
        this.avatarHeight = avatarHeight;
        XRRig rig = FindObjectOfType<XRRig>();
        playerHeight= rig.transform.Find("Camera Offset/Main Camera").position.y;
        float offsetY = playerHeight - avatarHeight;
        transform.position = new Vector3(0, offsetY, 0);
    }
}
