using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIK : MonoBehaviour {

    public Animator playerAnim;
    public Transform hitStartCheckPos, hitTargetPosL, hitTargetPosR, hitTargetPosF;
    public LayerMask wallLayerMask;
    Vector3 leftHitPos, rightHitPos, fHitPos;
    float leftHitPosWeight, rightHitPosWeight, fHitPosWeight;
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, leftHitPosWeight);
        Gizmos.DrawLine(hitStartCheckPos.position, leftHitPos);

        Gizmos.color = new Color(0, 1, 0, rightHitPosWeight);
        Gizmos.DrawLine(hitStartCheckPos.position, rightHitPos);

        Gizmos.color = new Color(1, 1, 1, fHitPosWeight);
        Gizmos.DrawLine(hitStartCheckPos.position, fHitPos);
    }
	
	// Update is called once per frame
	void Update ()
    {
        // -------------------------------- check right, left of player -------------- // 
        // check right hand
        RaycastHit rHit;
        var rDir = hitTargetPosR.position - hitStartCheckPos.position;
        Physics.Raycast(hitStartCheckPos.position, rDir, out rHit, rDir.magnitude, wallLayerMask, QueryTriggerInteraction.Ignore);
        if(rHit.collider != null)
        {
            rightHitPos = rHit.point;
            // set weight to increase as the distance to the collider that was hit decreases
            var actualRDist = (rightHitPos - hitStartCheckPos.position).magnitude;
            rightHitPosWeight = Mathf.Max(0, 1f - (actualRDist / (rDir.magnitude-.5f)));
        }
        else { rightHitPosWeight = 0; }
        // check left hand
        RaycastHit lHit;
        var lDir = hitTargetPosL.position - hitStartCheckPos.position;
        Physics.Raycast(hitStartCheckPos.position, lDir, out lHit, lDir.magnitude, wallLayerMask, QueryTriggerInteraction.Ignore);
        if (lHit.collider != null)
        {
            leftHitPos = lHit.point;
            // set weight to increase as the distance to the collider that was hit decreases
            var actualLDist = (leftHitPos - hitStartCheckPos.position).magnitude;
            leftHitPosWeight = 1f - (actualLDist / (lDir.magnitude - .5f));
        }
        else { leftHitPosWeight = 0; }
        // -------------------------------- check in front of player -------------- // 
        // check front 
        RaycastHit fHit;
        var fDir = hitTargetPosF.position - hitStartCheckPos.position;
        Physics.Raycast(hitStartCheckPos.position, fDir, out fHit, fDir.magnitude, wallLayerMask, QueryTriggerInteraction.Ignore);
        if (fHit.collider != null)
        {
            fHitPos = fHit.point;
            // set weight to increase as the distance to the collider that was hit decreases
            var actualFDist = (fHitPos - hitStartCheckPos.position).magnitude;
            fHitPosWeight = 1f - (actualFDist / (fDir.magnitude - .5f));
        }
        else { fHitPosWeight = 0; }
    }


    private void OnAnimatorIK(int layerIndex)
    {
        // order matters here apparently!!
        // these side IK targets won't do anything if they are overridden (like they are below by the front IK target)
        if (PlayerController.instance.isOnGround)
        {
            playerAnim.SetIKPosition(AvatarIKGoal.RightHand, rightHitPos);
            playerAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHitPosWeight);
            playerAnim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightHitPosWeight);
            playerAnim.SetIKHintPosition(AvatarIKHint.RightElbow, transform.position + Vector3.down);

            playerAnim.SetIKPosition(AvatarIKGoal.LeftHand, leftHitPos);
            playerAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHitPosWeight);
            playerAnim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftHitPosWeight);
            playerAnim.SetIKHintPosition(AvatarIKHint.LeftElbow, transform.position + Vector3.down);
        }

        // have the forward collisions override the others
        playerAnim.SetIKPosition(AvatarIKGoal.RightHand, fHitPos);
        playerAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, fHitPosWeight);
        playerAnim.SetIKPosition(AvatarIKGoal.LeftHand, fHitPos);
        playerAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, fHitPosWeight);
        playerAnim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, fHitPosWeight);
    }
}
