using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public GameCharacter gameCharacter;
    public string moveInputHorz;
    public string moveInputVert;
    public string jumpInput;
	
	// Update is called once per frame
	void Update () {
        gameCharacter.Input.Move = new Vector2(Input.GetAxis(moveInputHorz), Input.GetAxis(moveInputVert));
        gameCharacter.Input.Jump = Input.GetButton(jumpInput);
	}
}
