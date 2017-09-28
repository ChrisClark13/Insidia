using UnityEngine;
using System.Collections;

/// <summary>
/// A base class for ScriptableObjects that contain coroutines that can make an AI character navigate a <see cref="NavMeshLink"/> that requires custom behaviour to cross.
/// <para>To use, create a subclass script and override <see cref="FollowLink(AICharMovement)"/>. Look at existing scripts for examples on how to do it. aiCharMovement.Agent.CompleteOffMeshLink() MUST be called at some point or else the AI character will bug out.</para>
/// <para>Created by Christian Clark</para>
/// </summary>
public abstract class CustomNavLinkBehaviour : ScriptableObject
{
    public abstract IEnumerator FollowLink(AICharMovement aiCharMovement);
}
