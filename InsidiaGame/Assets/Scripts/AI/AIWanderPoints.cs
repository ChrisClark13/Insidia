using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Points for AI charaters using the <see cref="AIBrainWander"/> script to follow. Only place one in a Scene and fill out its Points list with Transforms.
/// Created by Christian Clark
/// </summary>
public class AIWanderPoints : MonoBehaviour {

	public static Transform[] Points
    {
        get
        {
            if (_instance)
                return _instance.points;
            else
                return null;
        }
    }

    private static AIWanderPoints _instance;

    public Transform[] points;

    private void Awake()
    {
        _instance = this;
    }
}
