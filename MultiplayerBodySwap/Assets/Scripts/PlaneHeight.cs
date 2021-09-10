using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class PlaneHeight : MonoBehaviour
{
    public float player1Height = 0;
    public float player2Height = 0;
    public float avatar1Height = 1.7f;
    public float avatar2Height = 1.7f;
    public bool adaptToMaxOffset;

    public void heightCalibration(float avatarHeight)
    {
        if (player1Height == 0)
        {
            this.avatar1Height = avatarHeight;
            XRRig rig = FindObjectOfType<XRRig>();
            player1Height = rig.transform.Find("Camera Offset/Main Camera").position.y;
        }
        else
        {
            this.avatar2Height = avatarHeight;
            XRRig rig = FindObjectOfType<XRRig>();
            player2Height = rig.transform.Find("Camera Offset/Main Camera").position.y;

            //change plane height
            float offsetY1 = player1Height - avatar1Height;
            float offsetY2 = player2Height - avatar2Height;
            float offset = (adaptToMaxOffset) ? Mathf.Max(offsetY1, offsetY2) : Mathf.Min(offsetY1, offsetY2);
            transform.position = new Vector3(0, offset, 0);
        }
    }
}
