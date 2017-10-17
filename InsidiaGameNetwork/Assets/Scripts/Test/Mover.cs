using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just about the simplest character controller based movement script possible. For testing purposes only.
/// Created by Christian Clark
/// </summary>
public class Mover : MonoBehaviour {

    public string horzInput = "Horizontal";
    public string vertInput = "Vertical";
    public float moveSpeed = 10f;
    public float rotationSpeed = 120f;
    public string jumpInput = "Jump";
    public float jumpForce = 10f;
    public float gravityAcceleration = 10f;
    public float terminalVelocity = 40f;
    private float _yVelocity = 0f;
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(Vector3.up, Input.GetAxis(horzInput) * rotationSpeed * Time.fixedDeltaTime);
        Vector3 forwardVelocity = transform.forward * Input.GetAxis(vertInput) * moveSpeed;

        if ((_characterController.collisionFlags & CollisionFlags.Below) != 0)
        {
            _yVelocity = 0f;
            if (Input.GetButtonDown(jumpInput))
            {
                _yVelocity += jumpForce;
            }
        }
        _yVelocity = Mathf.Max(-terminalVelocity, _yVelocity -= gravityAcceleration * Time.fixedDeltaTime);

        _characterController.Move((forwardVelocity + Vector3.up * _yVelocity) * Time.fixedDeltaTime);
	}
}
