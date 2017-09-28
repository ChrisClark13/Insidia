  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place this on any game object and point it to whatever GameCharacter script needs player input from the keyboard.
/// Fill in the strings with the names of the input axes from Unity's InputManager.
/// <para>To be used for testing purposes.</para>
/// <para>Created by Christian Clark</para>
/// </summary>
public class KeyboardInput : MonoBehaviour {

    public CharacterInput characterInput;
    public Camera cam;
    public float vertCamOffset = 2f;
    public float camDistance = 5f;
    public string moveInputHorz = "Horizontal";
    public string moveInputVert = "Vertical";
    public string aimInputHorz = "Mouse X";
    public string aimInputVert = "Mouse Y";
    private float camRotationY;
    private float camRotationX;
    [Range(0.1f, 10f)]
    public float aimSensitivity = 1;
    public string jumpInput = "Jump";
    public string attackInput = "Fire1";
    public string attackModeInput = "Fire3";
    public string strafingInput = "Fire2";

    public GameObject aimPointMarker = null;

    // Update is called once per frame
    // Must be called each frame or else inputs could be missed.
    // At the moment there is no way around this (without making controlls laggy/miss inputs), but a new input system for Unity should come out soon.

    private void Start()
    {
        cam = (Camera.main != null) ? Camera.main : cam;
        camRotationY = cam.transform.eulerAngles.y;
        camRotationX = cam.transform.eulerAngles.x;
    }

    void Update () {
        // Process Move Input

        Vector3 moveInput = new Vector3(Input.GetAxis(moveInputHorz), 0, Input.GetAxis(moveInputVert));
        //Rotate the moveInput so that it's relative to the camera;
        moveInput = Quaternion.Euler(0, camRotationY, 0) * moveInput;
        //Set Input
        characterInput.Move.Value = new Vector2(moveInput.x, moveInput.z);

        // Set Button Inputs

        characterInput.Jump.Value = Input.GetButton(jumpInput);
        characterInput.Attack.Value = Input.GetButton(attackInput);
        characterInput.AttackMode.Value = Input.GetButton(attackModeInput);
        characterInput.LockOn.Value = Input.GetButton(strafingInput);
	}

    private void LateUpdate()
    {
        // Process Aim Input and Camera controls
        // Done inside LateUpdate to avoid gitteriness

        camRotationY += Input.GetAxis(aimInputHorz) * aimSensitivity;
        camRotationX += Input.GetAxis(aimInputVert) * aimSensitivity;
        camRotationX = Mathf.Clamp(camRotationX, -70f, 90f);
        cam.transform.eulerAngles = new Vector3(camRotationX, camRotationY, 0);
        Vector3 camOffset = cam.transform.rotation * (Vector3.back * camDistance);
        camOffset.y += vertCamOffset;
        cam.transform.position = camOffset + characterInput.transform.position;

        // Get Aim Point

        //Do a raycast from the camera against the entire scene and find the closest thing that isn't just ourselves

        //This is the most accurate way to do it, but it's definately not the cheapest.
        bool pointFound = false;
        Vector3 aimPoint = Vector3.zero;
        // Used both because this is a script that will be replaced and because this is the most reliable way to get the closest point to the camera.
        // Might be possible to be replace this by swinging a trigger collider, that'll generate more code, complexity, and possible innaccuracy.
        RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, cam.transform.forward);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(characterInput.transform))
            {
                if (!pointFound)
                {
                    aimPoint = hit.point;
                    pointFound = true;
                }
                else if (Vector3.Distance(characterInput.transform.position, aimPoint) > Vector3.Distance(characterInput.transform.position, hit.point))
                    aimPoint = hit.point;
            }
        }

        if (!pointFound)
        {
            //Doing this all the time would be the cheapest way to do it.
            aimPoint = cam.transform.position + (cam.transform.forward * 100f);
        }

        //Set Input now that we have the point we're aiming at.
        characterInput.AimPoint.Value = aimPoint;
        if (aimPointMarker != null)
            aimPointMarker.transform.position = aimPoint;
    }
}
