using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Place things in the Behaviour or GameObject list in the editor. Check the tick boxes to set where that stuff should be allowed to run.
/// <para>You may need to widen the inspector.</para>
/// <para></para>Created by Christian Clark.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class NetworkDisabler : NetworkBehaviour {

    [Serializable]
    public class BehaviourEntry
    {
        public Behaviour behaviour;
        public bool disableIfNotLocal = true;
        public bool disableIfNotClient = false;
        public bool disableIfNotServer = false;
    }

    [Serializable]
    public class GameObjectEntry
    {
        public GameObject gameObject;
        public bool disableIfNotLocal = true;
        public bool disableIfNotClient = false;
        public bool disableIfNotServer = false;
    }

    public List<BehaviourEntry> behavioursToDisable = new List<BehaviourEntry>();
    public List<GameObjectEntry> gameObjectsToDisable = new List<GameObjectEntry>();

	// Use this for initialization
	void Start () {
        //Note, both isClient and isServer will return as true if the player is running the server as it's host. This is fine.

        foreach (var entry in behavioursToDisable)
        {
            entry.behaviour.enabled = (entry.disableIfNotLocal && isLocalPlayer) || (entry.disableIfNotClient && isClient) || (entry.disableIfNotServer && isServer);
        }

        foreach (var entry in gameObjectsToDisable)
        {
            entry.gameObject.SetActive((entry.disableIfNotLocal && isLocalPlayer) || (entry.disableIfNotClient && isClient) || (entry.disableIfNotServer && isServer));
        }
    }
}
