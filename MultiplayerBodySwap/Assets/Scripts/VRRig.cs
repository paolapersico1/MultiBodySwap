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
    public float avatarHeadHeight;
    public float avatarArmLength;
    public Vector3 avatarEyeHeadOffset;
    
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    private Transform vrHead;
    private Transform vrLeftHand;
    private Transform vrRightHand;

    public bool bodyRotation = true;
    public int rotThreshold = 10;
    public float turnSmoothness;

    private PhotonView photonView;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        XRRig rig = FindObjectOfType<XRRig>();
        vrHead = rig.transform.Find("Camera Offset/Main Camera");
        vrLeftHand = rig.transform.Find("Camera Offset/LeftHand Controller");
        vrRightHand = rig.transform.Find("Camera Offset/RightHand Controller");

        Calibrate(rig);
    }

    void Calibrate(XRRig rig)
    {
        float playerHeadHeight = rig.GetComponent<Calibrator>().GetPlayerHeadHeight();
        float playerArmLength = rig.GetComponent<Calibrator>().GetPlayerArmLength();

        //how much taller is the avatar
        avatarHeadHeight = head.rigTarget.position.y - transform.position.y;
        float offset = avatarHeadHeight - playerHeadHeight;
        Transform cameraOffset = rig.transform.Find("Camera Offset");
        cameraOffset.position = new Vector3(0, offset, 0);

        //how longer are the avatar's arms
        avatarArmLength = Vector3.Distance(rightHand.rigTarget.position, head.rigTarget.position);
        offset = avatarArmLength - playerArmLength;
        leftHand.trackingPositionOffset = new Vector3(0, 0, offset);
        rightHand.trackingPositionOffset = new Vector3(0, 0, offset);

        Transform cam = rig.transform.Find("Camera Offset/Main Camera/Camera");
        cam.localPosition = avatarEyeHeadOffset;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (photonView.IsMine)
        {
            if (vrRightHand)
            {
                float reach = animator.GetFloat("RightHand");
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reach);
                animator.SetIKPosition(AvatarIKGoal.RightHand, vrRightHand.TransformPoint(rightHand.trackingPositionOffset));
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, reach);
                animator.SetIKRotation(AvatarIKGoal.RightHand, vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset));

            }

            if (vrLeftHand)
            {
                float reach = animator.GetFloat("LeftHand");
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, reach);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, vrLeftHand.TransformPoint(leftHand.trackingPositionOffset));
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, reach);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, vrLeftHand.rotation * Quaternion.Euler(leftHand.trackingRotationOffset));

            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = head.rigTarget.position - new Vector3(0, avatarHeadHeight, 0);
            //transform.forward = Vector3.ProjectOnPlane(head.rigTarget.transform.up, Vector3.up).normalized;
            //float headTorsoRotation = RotationDifference(transform, head.rigTarget);
            float leftHandTorsoRotation = RotationDifference(transform, vrLeftHand);
            float rightHandTorsoRotation = RotationDifference(transform, vrRightHand);
            if (bodyRotation && 
                leftHandTorsoRotation > rotThreshold &&
                rightHandTorsoRotation > rotThreshold)
            {
                transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(head.rigTarget.forward, Vector3.up).normalized,
                                            Time.deltaTime * turnSmoothness);
            }

            head.Map(vrHead);
            //leftHand.Map(vrLeftHand);
            //rightHand.Map(vrRightHand);
        }
    }

    private float RotationDifference(Transform firstTransform, Transform secondTransform)
    {
        return Quaternion.Angle(Quaternion.Euler(0, firstTransform.rotation.eulerAngles.y, 0),
                Quaternion.Euler(0, secondTransform.rotation.eulerAngles.y, 0));
    }
}
