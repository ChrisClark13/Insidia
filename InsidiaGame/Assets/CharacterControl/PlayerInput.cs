using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public GameCharacter gameCharacter;
    public string moveInputHorz;
    public string moveInputVert;
    public string aimInputHorz;
    public string aimInputVert;
    public string jumpInput;
    public string sprintInput;
	
	// Update is called once per frame
    // Must be called each frame or else inputs could be missed.
    // At the moment there is no way around this (without making controlls laggy/miss inputs), but a new input system for Unity should come out soon.
	void Update () {
        gameCharacter.Input.Move.Value = new Vector2(Input.GetAxis(moveInputHorz), Input.GetAxis(moveInputVert));
        gameCharacter.Input.Aim.Value = new Vector2(Input.GetAxis(aimInputHorz), Input.GetAxis(aimInputVert));
        gameCharacter.Input.Jump.Value = Input.GetButton(jumpInput);
        gameCharacter.Input.Sprint.Value = Input.GetButton(sprintInput);
	}
}
