using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "ArchTeam/Custom NavLink Behaviours/Warp")]
public class WarpLinkBehaviour : CustomNavLinkBehaviour
{
    public override IEnumerator FollowLink(AICharMovement aiCharMovement)
    {
        CharacterController characterController = aiCharMovement.GetComponent<CharacterController>();

        aiCharMovement.linkFollowGoal = aiCharMovement.Agent.currentOffMeshLinkData.startPos;
        yield return new WaitUntil(() => aiCharMovement.IsAtLinkFollowGoal(characterController.radius * 2.25f));

        aiCharMovement.transform.position = aiCharMovement.Agent.currentOffMeshLinkData.endPos;
        aiCharMovement.transform.Translate(0, characterController.height / 2f, 0, Space.World);
        
        aiCharMovement.Agent.nextPosition = aiCharMovement.transform.position;
    }
}
