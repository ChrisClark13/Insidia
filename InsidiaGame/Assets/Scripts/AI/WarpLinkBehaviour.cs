using UnityEngine;
using System.Collections;

/// <summary>
/// A bit of a silly script I don't expect to be actually used in game. Warps an AI Character from the start to the end of the link instantly.
/// <para>Created by Christian Clark</para>
/// </summary>
[CreateAssetMenu(menuName = "ArchTeam/Custom NavLink Behaviours/Warp")]
public class WarpLinkBehaviour : CustomNavLinkBehaviour
{
    public override IEnumerator FollowLink(AICharMovement aiCharMovement)
    {
        CharacterController characterController = aiCharMovement.GetComponent<CharacterController>();

        aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.startPos;
        yield return new WaitUntil(() => aiCharMovement.IsAtLinkFollowGoal(aiCharMovement.Agent.radius * 2.25f));
        

        aiCharMovement.transform.position = aiCharMovement.Agent.currentOffMeshLinkData.endPos;
        aiCharMovement.transform.Translate(0, aiCharMovement.Agent.baseOffset, 0, Space.World);
        
        aiCharMovement.Agent.nextPosition = aiCharMovement.transform.position;

        aiCharMovement.Agent.CompleteOffMeshLink();
    }
}
