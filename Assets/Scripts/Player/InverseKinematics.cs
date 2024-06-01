using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour
{
    public Animator animator;

    public Transform rightHandObj = null;
    public Transform leftHandObj = null;
    public Transform rightFootObj = null;
    public Transform leftFootObj = null;
    public Transform lookObj = null;

    // A callback for calculating IK
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            UpdateIKTargets();
        }
    }

    private void UpdateIKTargets()
    {
        if (lookObj != null)
        {
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(lookObj.position);
        }

        if (rightHandObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
        }

        if (leftHandObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
        }

        if (rightFootObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootObj.position);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootObj.rotation);
        }

        if (leftFootObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
        }
    }
}
