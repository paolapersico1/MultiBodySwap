using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class Calibrator : MonoBehaviour
{
    public float playerHeadHeight;
    public float playerArmLength;

    public Transform vrHead;
    public Transform vrRightHand;

    public float hmdHeadRootOffset = 0.1f; //hmd usually 10cm above head root
    public float hmdHeadTopOffset = 0.05f; //hmd usually 5cm below head top
    public float handWristOffset = 0.15f;  //hand length

    public void SetPlayerParams()
    {
        //distance between head root and ground
        playerHeadHeight = vrHead.position.y - hmdHeadRootOffset;
        //distance between head root and right hand
        float playerHeight = vrHead.position.y + hmdHeadTopOffset;
        playerArmLength = (playerHeight / 2.0f) - handWristOffset;
    }

    public float GetPlayerHeadHeight()
    {
        return playerHeadHeight;
    }

    public float GetPlayerArmLength()
    {
        return playerArmLength;
    }
}
