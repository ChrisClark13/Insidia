using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

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
