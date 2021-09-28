using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map(Transform vrTransform)
    {
        vrTarget = vrTransform;
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }

}
public class VRRig : MonoBehaviour
{
    public float avatarHeight;
    public float avatarHeadHeight;
    public float avatarArmLength;
    public Vector3 avatarEyeHeadOffset;
    
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;
    public Transform headTop;

    private Transform vrHead;
    private Transform vrLeftHand;
    private Transform vrRightHand;

    public bool bodyRotation = true;
    public int rotThreshold;
    public float turnSmoothness;
    public float handsTorsoRotation; //DEBUG
    public float crouchingThreshold;
    public float bendingThreshold;

    private PhotonView photonView;
    private Animator animator;
    private float playerArmLength;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        XRRig rig = FindObjectOfType<XRRig>();
        vrHead = rig.transform.Find("Camera Offset/Main Camera");
        vrLeftHand = rig.transform.Find("Camera Offset/LeftHand Controller");
        vrRightHand = rig.transform.Find("Camera Offset/RightHand Controller");

        if (photonView.IsMine)
        {
            Calibrate(GameObject.Find("XR Rig/Camera Offset"));
        }
    }

    void Calibrate(GameObject camOffset)
    {
        float playerHeadHeight = camOffset.GetComponent<Calibrator>().GetPlayerHeadHeight();
        playerArmLength = camOffset.GetComponent<Calibrator>().GetPlayerArmLength();

        //how much taller is the avatar
        avatarHeight = headTop.position.y - transform.position.y;

        avatarHeadHeight = head.rigTarget.position.y - transform.position.y;
        camOffset.transform.position = new Vector3(0, avatarHeadHeight - playerHeadHeight, 0);

        //how longer are the avatar's arms
        avatarArmLength = Vector3.Distance(rightHand.rigTarget.position, head.rigTarget.position);
        float armOffset = avatarArmLength - playerArmLength;
        leftHand.trackingPositionOffset = new Vector3(0, 0, armOffset);
        rightHand.trackingPositionOffset = new Vector3(0, 0, armOffset);

        Transform cam = camOffset.transform.Find("Main Camera/Camera");
        cam.localPosition = avatarEyeHeadOffset;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (photonView.IsMine)
        {
            float bendingPoint = playerArmLength / bendingThreshold;
            if (vrRightHand)
            {
                float reach = animator.GetFloat("RightHand");
                bool isArmBended = IsArmBended(vrRightHand, bendingPoint);
                Vector3 goalPosition = (isArmBended) ? vrRightHand.TransformPoint(rightHand.trackingPositionOffset) : vrRightHand.position;
                animator.SetFloat("rightHandX", goalPosition.x);
                animator.SetFloat("rightHandY", goalPosition.y);
                animator.SetFloat("rightHandZ", goalPosition.z);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reach);
                animator.SetIKPosition(AvatarIKGoal.RightHand, goalPosition);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, reach);
                animator.SetIKRotation(AvatarIKGoal.RightHand, vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset));

            }

            if (vrLeftHand)
            {
                float reach = animator.GetFloat("LeftHand");
                bool isArmBended = IsArmBended(vrLeftHand, bendingPoint);
                Vector3 goalPosition = (isArmBended) ? vrLeftHand.TransformPoint(leftHand.trackingPositionOffset) : vrLeftHand.position;
                animator.SetFloat("leftHandX", goalPosition.x);
                animator.SetFloat("leftHandY", goalPosition.y);
                animator.SetFloat("leftHandZ", goalPosition.z);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, reach);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, goalPosition);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, reach);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, vrLeftHand.rotation * Quaternion.Euler(leftHand.trackingRotationOffset));
            }
        }
        else
        {
            float reach = animator.GetFloat("RightHand");
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reach);
            Vector3 goalPosition = new Vector3(animator.GetFloat("rightHandX"), 
                                                animator.GetFloat("rightHandY"), 
                                                animator.GetFloat("rightHandZ"));
            animator.SetIKPosition(AvatarIKGoal.RightHand, goalPosition);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, reach);
            //animator.SetIKRotation(AvatarIKGoal.RightHand, vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset));

            reach = animator.GetFloat("LeftHand");
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, reach);
            goalPosition = new Vector3(animator.GetFloat("leftHandX"),
                                                animator.GetFloat("leftHandY"),
                                                animator.GetFloat("leftHandZ"));
            animator.SetIKPosition(AvatarIKGoal.LeftHand, goalPosition);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, reach);
            //animator.SetIKRotation(AvatarIKGoal.LeftHand, vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset));
        }
    }

    // Update is called once per frame
    void OnAnimatorMove()
    {
        if (photonView.IsMine)
        {
            Vector3 handsPosition = Vector3.Lerp(rightHand.rigTarget.position, leftHand.rigTarget.position, 0.5f);
            Vector3 newTorsoPosition = head.rigTarget.position - new Vector3(0, avatarHeadHeight, 0);

            transform.position = new Vector3(newTorsoPosition.x, 
                                            IsCrouched(head.rigTarget.position)? newTorsoPosition.y : transform.position.y, 
                                            newTorsoPosition.z);

            handsTorsoRotation = Vector3.Angle(
                                    Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized,
                                    Vector3.ProjectOnPlane(handsPosition - transform.position, Vector3.up).normalized
                                    );

            if (bodyRotation && handsTorsoRotation > rotThreshold)
            {
                //transform.forward = Vector3.ProjectOnPlane(head.rigTarget.transform.up, Vector3.up).normalized;
                transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(head.rigTarget.forward, Vector3.up).normalized,
                                            Time.deltaTime * turnSmoothness);
            }
            head.Map(vrHead);
        }
    }

    private bool IsCrouched(Vector3 headPosition)
    {
        RaycastHit hit;
        Ray ray = new Ray(headPosition, Vector3.down);

        if(Physics.Raycast(ray, out hit))
        {
            float distance = Vector3.Distance(headPosition, hit.transform.position);
            if (distance < (avatarHeight * crouchingThreshold))
                return false;
        }

        return true;
    }

    private bool IsArmBended(Transform vrHand, float bendingPoint)
    {
        return (Mathf.Abs(vrHand.position.z - transform.position.z) < bendingPoint);
    }
}
