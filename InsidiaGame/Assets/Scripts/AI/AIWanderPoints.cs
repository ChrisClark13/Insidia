using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
