using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This script is necessary for AI characters to get around the level in more complex ways than simply jumping up steps and dropping down ledges. Its list holds pairs of <see cref="NavMeshLink"/>s and <see cref="CustomNavLinkBehaviour"/>s.
/// <para>When it tries to follow a link in the list, it will use the custom behaviour specified.</para>
/// <para>To use: Place on a GameObject with <see cref="NavMeshLink"/> components/scripts on it and assign all of them to a spot in the list. Then assign a <see cref="CustomNavLinkBehaviour"/> to each one as needed (use the dot in the inspector to see what you can assign or write your own!).
/// <para>Created by Christian Clark</para></summary>
public class CustomNavLinkManager : MonoBehaviour
{
    [Serializable]
    public class LinkEntry
    {
        public NavMeshLink link;
        public CustomNavLinkBehaviour behaviour;
    }

    public List<LinkEntry> linkBehaviours = new List<LinkEntry>();

    public CustomNavLinkBehaviour GetBehaviour(NavMeshLink link)
    {
        return linkBehaviours.Find((entry) => entry.link == link).behaviour;
    }
}
