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
    //public bool collisionMode;

    public float avatarHeadHeight;
    public float avatarArmLength;

    public Vector3 avatarEyeHeadOffset;
    
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Transform leftHandCollider;
    public Transform rightHandCollider;
    public bool isReplica;

    private Transform vrHead;
    private Transform vrLeftHand;
    private Transform vrRightHand;

    public bool bodyRotation = true;
    public int rotThreshold;
    public float turnSmoothness;
    public float crouchingThreshold;
    public float crouchSmoothness;
    public LayerMask floorLayer;

    //DEBUG
    public float handsTorsoRotation;
    public bool isLookingDown;

    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private PhotonView photonView;
    private Animator animator;

    private float playerArmLength;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        XRRig rig = FindObjectOfType<XRRig>();
        vrHead = rig.transform.Find("Player Offset/Camera Offset/Main Camera");
        vrLeftHand = rig.transform.Find("Player Offset/Camera Offset/LeftHand Controller");
        vrRightHand = rig.transform.Find("Player Offset/Camera Offset/RightHand Controller");

        head.vrTarget = vrHead;
        leftHand.vrTarget = vrLeftHand;
        rightHand.vrTarget = vrRightHand;

        head.vrTarget = vrHead;
        leftHand.vrTarget = vrLeftHand;
        rightHand.vrTarget = vrRightHand;

        if (photonView.IsMine)
        {
            GameObject playerOffset = GameObject.Find("XR Rig/Player Offset");

            float playerHeadHeight = playerOffset.GetComponent<Calibrator>().GetPlayerHeadHeight();
            playerArmLength = playerOffset.GetComponent<Calibrator>().GetPlayerArmLength();

            avatarHeadHeight = head.rigTarget.position.y - transform.position.y;
            Debug.Log("Height difference: " + (avatarHeadHeight - playerHeadHeight));
            playerOffset.transform.position = new Vector3(0, avatarHeadHeight - playerHeadHeight, 0);

            //how longer are the avatar's arms
            avatarArmLength = Vector3.Distance(rightHand.rigTarget.position, head.rigTarget.position);
            float armOffset = (avatarArmLength - playerArmLength) / 2.0f;
            leftHand.trackingPositionOffset = new Vector3(0, 0, armOffset);
            rightHand.trackingPositionOffset = new Vector3(0, 0, armOffset);
            //leftHand.vrTarget.localPosition = leftHand.trackingPositionOffset;
            //rightHand.vrTarget.localPosition = rightHand.trackingPositionOffset;

            Transform cam = vrHead.transform.Find("Camera");
            cam.localPosition = avatarEyeHeadOffset;

            if (!isReplica)
            {
                rightHandCollider = GameObject.FindWithTag("righthand" + this.name.Replace("(Clone)", "")).transform;
                leftHandCollider = GameObject.FindWithTag("lefthand" + this.name.Replace("(Clone)", "")).transform;
            } 
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (photonView.IsMine)
        {
            if (isReplica)
            {
                //Debug.Log("replica");
                Vector3 goalPosition = vrRightHand.TransformPoint(rightHand.trackingPositionOffset);
                rightHandTarget.position = goalPosition;
                Quaternion goalRotation = vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset);
                rightHandTarget.rotation = goalRotation;

                goalPosition = vrLeftHand.TransformPoint(leftHand.trackingPositionOffset);
                leftHandTarget.position = goalPosition;
                goalRotation = vrLeftHand.rotation * Quaternion.Euler(leftHand.trackingRotationOffset);
                leftHandTarget.rotation = goalRotation;
            }
            else
            {
                //Debug.Log("not replica");
                if (!HasCollided(rightHandCollider))
                {
                    Vector3 goalPosition = vrRightHand.TransformPoint(rightHand.trackingPositionOffset);
                    rightHandTarget.position = goalPosition;
                    Quaternion goalRotation = vrRightHand.rotation * Quaternion.Euler(rightHand.trackingRotationOffset);
                    rightHandTarget.rotation = goalRotation;
                }
                else
                {
                    Debug.Log("right");
                }

                if (!HasCollided(leftHandCollider))
                {
                    Vector3 goalPosition = vrLeftHand.TransformPoint(leftHand.trackingPositionOffset);
                    leftHandTarget.position = goalPosition;
                    Quaternion goalRotation = vrLeftHand.rotation * Quaternion.Euler(leftHand.trackingRotationOffset);
                    leftHandTarget.rotation = goalRotation;
                }
                else
                {
                    Debug.Log("left");
                }
            }
        }

        UpdateHand(true, rightHandTarget.position, rightHandTarget.rotation);
        UpdateHand(false, leftHandTarget.position, leftHandTarget.rotation);
    }

    // Update is called once per frame
    void OnAnimatorMove()
    {
        if (photonView.IsMine)
        {
            isLookingDown = IsLookingDown(head.rigTarget);

            transform.position = new Vector3(head.rigTarget.position.x,
                                        isLookingDown? transform.position.y :
                                            Mathf.Lerp(transform.position.y, head.rigTarget.position.y - avatarHeadHeight, Time.deltaTime * crouchSmoothness),
                                        head.rigTarget.position.z);

            Vector3 handsPosition = Vector3.Lerp(rightHand.rigTarget.position, leftHand.rigTarget.position, 0.5f);

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

    private void UpdateHand(bool isRightHand, Vector3 handPos, Quaternion handRot)
    {
        if ((isRightHand) ? vrRightHand : vrLeftHand)
        {
            float reach = animator.GetFloat((isRightHand) ? "RightHand" : "LeftHand");
            AvatarIKGoal ikGoal = (isRightHand) ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;

            animator.SetIKPositionWeight(ikGoal, reach);
            animator.SetIKPosition(ikGoal, handPos);
            animator.SetIKRotationWeight(ikGoal, reach);
            animator.SetIKRotation(ikGoal, handRot);
        }
    }

   private bool IsLookingDown(Transform head)
    {
        RaycastHit hit;
        Ray ray = new Ray(head.position, head.forward);

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer.value))
        {
            float distance = Vector3.Distance(head.position, hit.point);
            if (distance < (avatarHeadHeight * crouchingThreshold))
                return true;
        }

        return false;
    }

    private bool HasCollided(Transform bodyPart) => bodyPart.gameObject.GetComponent<CollisionDetection>().HasCollided();
}
