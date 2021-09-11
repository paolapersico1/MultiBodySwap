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
    public float turnSmoothness;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    private Transform vrHead;
    private Transform vrLeftHand;
    private Transform vrRightHand;

    public Transform headConstraint;
    public Transform leftHandConstraint;
    public Transform rightHandConstraint;
    public Vector3 headBodyOffset;
    public bool bodyRotation;
    public int rotationDegrees = 90;

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        XRRig rig = FindObjectOfType<XRRig>();
        vrHead = rig.transform.Find("Camera Offset/Main Camera");
        vrLeftHand = rig.transform.Find("Camera Offset/LeftHand Controller");
        vrRightHand = rig.transform.Find("Camera Offset/RightHand Controller");

        headBodyOffset = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = headConstraint.position + headBodyOffset;
            //transform.forward = Vector3.ProjectOnPlane(head.rigTarget.transform.up, Vector3.up).normalized;
            if (bodyRotation && RotationGreaterThan(transform, headConstraint, rotationDegrees) &&
                RotationGreaterThan(transform, leftHandConstraint, rotationDegrees) && 
                RotationGreaterThan(transform, rightHandConstraint, rotationDegrees))
            {
                transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized,
                                            Time.deltaTime * turnSmoothness);
            }

            head.Map(vrHead);
            leftHand.Map(vrLeftHand);
            rightHand.Map(vrRightHand);
        }
    }

    private bool RotationGreaterThan(Transform firstTransform, Transform secondTransform, int degrees)
    {
        return Quaternion.Angle(Quaternion.Euler(0, firstTransform.rotation.eulerAngles.y, 0),
                Quaternion.Euler(0, secondTransform.rotation.eulerAngles.y, 0)) > degrees;
    }
}
