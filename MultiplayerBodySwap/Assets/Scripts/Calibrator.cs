using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class Calibrator : MonoBehaviour
{
    public bool vrSimulator = false;

    public float playerHeadHeight;
    public float playerArmLength;

    public Transform vrHead;
    public Transform vrRightHand;

    public float hmdHeadRootOffset = 0.03f;  //hmd usually 5cm above head root
    public float handWristOffset = 0.18f;  //hand length

    public void SetPlayerParams()
    {
        if (vrSimulator)
        {
            playerHeadHeight = 1.5f;
            playerArmLength = 0.75f;
        }
        else
        {
            playerHeadHeight = vrHead.position.y - hmdHeadRootOffset;
            playerArmLength = (vrHead.position.y / 2.0f) - handWristOffset;
        }
        
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
