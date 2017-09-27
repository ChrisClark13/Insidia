using UnityEngine;
using System.Collections;

public abstract class CustomNavLinkBehaviour : ScriptableObject
{
    public abstract IEnumerator FollowLink(AICharMovement aiCharMovement);
}
