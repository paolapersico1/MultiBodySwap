using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class CameraHeight : MonoBehaviour
{
    public float playerHeight = 0;
    public float avatarHeight = 1.7f;

    public void cameraOffsetCalibration(float avatarHeight)
    {
        this.avatarHeight = avatarHeight;
        XRRig rig = FindObjectOfType<XRRig>();
        playerHeight = rig.transform.Find("Camera Offset/Main Camera").position.y;

        //how much taller is the avatar
        float offset =  avatarHeight - playerHeight;
        transform.position = new Vector3(0, offset, 0);
    }
}
