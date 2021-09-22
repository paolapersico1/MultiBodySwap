using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class Calibrator : MonoBehaviour
{
    public float playerHeight;
    public float playerArmLength;

    public Transform vrHead;
    public Transform vrRightHand;

    public void SetPlayerParams()
    {
        playerHeight = vrHead.position.y;
        playerArmLength = Mathf.Abs(vrRightHand.position.z - vrHead.position.z);
    }

    public float GetPlayerHeight()
    {
        return playerHeight;
    }

    public float GetPlayerArmLength()
    {
        return playerArmLength;
    }
}
