using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AnimationsGirl : NetworkBehaviour
{
    private Animator animator;
    public GameObject cam;
    public float smooth = 0.5f;

    Rigidbody rb;

    private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition,rightFootIkPosition;
    private Quaternion leftFootIkRotation,rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    [Range(0,2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0,2)] [SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float pelvisOffset =0f;
    [Range(0,1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0,1)] [SerializeField] private float feetToIkPosistionSpeed = 0.5f;
    private float leftFootIkWeight;
    private float rightFootIkWeight;
    [Range(1,10)] [SerializeField] private float FootIkSensitivityRotation = 4;
    public string leftFootAnimationVariableName = "LeftFootCurve";
    public string rightFootAnimationVariableName= "RightFootCurve";
    public bool useProIkFeature = false;
    public bool showSolverDebug = true;

    void Start()
    {
        if(!IsOwner) return;
        Destroy(GameObject.FindGameObjectWithTag("MainCamera"));
        animator = gameObject.GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!IsOwner) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float ch = Input.GetAxis("LeftStickHorizontal");
        float cv = Input.GetAxis("LeftStickVertical");
        
        animator.SetFloat("Forward",v + cv);
        animator.SetFloat("Strafe",h + ch);

        if (v > 0.5f || cv > 0.5f) {
            Quaternion target = Quaternion.Euler(0, cam.transform.localRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
        }

        leftFootIkWeight = animator.GetFloat(leftFootAnimationVariableName);
        rightFootIkWeight = animator.GetFloat(rightFootAnimationVariableName);
    }

    private void FixedUpdate() {
        if(enableFeetIk == false) return;
        if(animator == null) return;

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        // find and raycast to the ground to find positions
        FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation, ref rightFootIkWeight); // handle the solver forr right foot
        FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation, ref leftFootIkWeight); // handle the solver left foot

        animator.SetFloat(leftFootAnimationVariableName, leftFootIkWeight);
        animator.SetFloat(rightFootAnimationVariableName, rightFootIkWeight);
    }

    private void OnAnimatorIK (int layerIndex) {
        if(enableFeetIk == false) return;
        if(animator == null) return;

        MovePelvisHeight();

        // Right foot ik position and rotation -- utilise the pro features in here
        animator.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);

        if(useProIkFeature) {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootIkWeight);
        }

        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

        // Left foot ik position and rotation -- utilise the pro features in here
        animator.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);

        if(useProIkFeature) {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootIkWeight);
        }

        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
    }

    void MoveFeetToIkPoint (AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY) {
        Vector3 targetIkPosition = animator.GetIKPosition(foot);

        if(positionIkHolder != Vector3.zero) {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPosistionSpeed);
            targetIkPosition.y += yVariable;
            
            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);
            animator.SetIKRotation(foot, rotationIkHolder);
        }
        animator.SetIKPosition(foot, targetIkPosition);
    }

    private void MovePelvisHeight() {
        if(rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0) {
            lastPelvisPositionY = animator.bodyPosition.y;
            return;
        }
        float lOffestPosition = leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffestPosition < rOffsetPosition) ? lOffestPosition : rOffsetPosition;

        Vector3 newPelvisPosition = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = animator.bodyPosition.y;
    }

    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotation, ref float feetIkWeight) {
        // raycast handling section
        RaycastHit feetOutHit;

        if(showSolverDebug) {
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        }
        if(Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer)) {
            // finding our feet ik positions from the sky position
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;

            feetIkWeight = ((Mathf.Abs(feetOutHit.normal.x) + Mathf.Abs(1-feetOutHit.normal.y) + Mathf.Abs(feetOutHit.normal.z))/3) * FootIkSensitivityRotation;
            feetIkRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }
        feetIkPositions = Vector3.zero; // it didn't work
    }

    private void AdjustFeetTarget (ref Vector3 feetPositions, HumanBodyBones foot) {
        feetPositions = animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }  
}
