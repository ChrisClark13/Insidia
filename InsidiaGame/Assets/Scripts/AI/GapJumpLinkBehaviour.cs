using UnityEngine;
using System.Collections;

/// <summary>
/// This script contains instructions on how to make an AI character jump across a gap. Use with <see cref="CustomNavLinkManager"/>. Will auto fail if the character is under the jump start point.
/// <para>Created by Christian Clark</para>
/// </summary>
[CreateAssetMenu(menuName = "ArchTeam/Custom NavLink Behaviours/Gap Jump")]
public class GapJumpLinkBehaviour : CustomNavLinkBehaviour
{
    public override IEnumerator FollowLink(AICharMovement aiCharMovement)
    {
        //Make sure we're not actually under the link start point (because the NavMeshAgent may have gotten confused about where we are and is trying to use the link when it shouldn't)
        if ((aiCharMovement.transform.position.y + aiCharMovement.Agent.baseOffset) >= aiCharMovement.Agent.currentOffMeshLinkData.startPos.y)
        {
            CharacterInput input = aiCharMovement.GetComponent<CharacterInput>();

            //Make sure you're touching the ground before you attempt to jump. Also, get yourself to the start point (mind the gap you're jumping over!)
            aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.startPos;
            yield return new WaitUntil(() => aiCharMovement.IsTouchingGround());

            //Go towards the end and JUMP!
            aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.endPos;
            input.Jump.Value = true;
            yield return new WaitForSeconds(0.1f);
            input.Jump.Value = false;

            //Free up the link for another AI character to use it.
            yield return new WaitUntil(() => aiCharMovement.GetHorizontalDistance(aiCharMovement.transform.position, aiCharMovement.Agent.currentOffMeshLinkData.startPos) >= aiCharMovement.Agent.radius * 1.5f);
            aiCharMovement.Agent.CompleteOffMeshLink();

            //Wait until we either reach the destination or touch the ground before exiting this behaviour.
            yield return new WaitUntil(() => aiCharMovement.IsTouchingGround() || aiCharMovement.IsAtLinkFollowGoal());
        }
        else
        {
            //Well, we auto failed, make sure to tell our Agent that we're done with the link.
            aiCharMovement.Agent.CompleteOffMeshLink();
        }
    }
}
