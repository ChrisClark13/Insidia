using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheatTester : MonoBehaviour {

    public GameCharacter character;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (character != null && Input.GetMouseButtonDown(0))
        {
            character.Heat = character.heatCapacity;
        }
	}
}
