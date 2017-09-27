using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "ArchTeam/Custom NavLink Behaviours/Gap Jump")]
public class GapJumpLinkBehaviour : CustomNavLinkBehaviour
{
    public override IEnumerator FollowLink(AICharMovement aiCharMovement)
    {
        if ((aiCharMovement.transform.position.y + aiCharMovement.GetComponent<CharacterController>().height/2f) >= aiCharMovement.Agent.currentOffMeshLinkData.startPos.y)
        {
            CharacterInput input = aiCharMovement.GetComponent<CharacterInput>();

            aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.startPos;
            yield return new WaitUntil(() => aiCharMovement.IsTouchingGround());

            aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.endPos;
            input.Jump.Value = true;
            yield return new WaitForSeconds(0.1f);
            input.Jump.Value = false;

            yield return new WaitUntil(() => aiCharMovement.IsTouchingGround() || aiCharMovement.IsAtLinkFollowGoal());
        }
    }
}
