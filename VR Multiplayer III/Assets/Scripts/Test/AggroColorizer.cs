using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AggroColorizer : MonoBehaviour {

    Minion minion;
    NetworkedHealth health;

	// Use this for initialization
	void Start () {
        minion = GetComponent<Minion>();
        health = GetComponent<NetworkedHealth>();
	}
	
	// Update is called once per frame
	void Update () {
        if (health.Health == 0)
        {
            GetComponent<Renderer>().material.color = Color.black;
        }
        else
        {
            var list = new List<float>(minion.aggroDict.Values);
            float highestAggro = (list.Count > 0) ? list.Max() : 0f;
            GetComponent<Renderer>().material.color = Color.Lerp(Color.green, Color.red, highestAggro / (minion.aggroProfile.maxAggro / 2f));
        }
	}
}
