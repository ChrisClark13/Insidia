using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleMotor : MonoBehaviour {

    public float speed = 5f;
    public float jumpStrength = 10f;
    public float yVelocity = 0f;
    public float gravity = 10f;
    public bool grounded = false;
    private CharacterController characterController;
    private GameCharacter gameCharacter;
    public float terminalVelocity = -20;

    public void Awake()
    {
        characterController = GetComponent<CharacterController>();
        gameCharacter = GetComponent<GameCharacter>();
    }

    public void OnEnable()
    {
        gameCharacter.OnMovementUpdate += Move;
        gameCharacter.Input.OnJumpChange += Jump;
    }

    public void OnDisable()
    {
        gameCharacter.OnMovementUpdate -= Move;
        gameCharacter.Input.OnJumpChange -= Jump;
    }

    private void Move(GameCharacter gameCharacter, float deltaTime)
    {
        Vector3 velocity = Vector3.zero;
        Vector3 moveInput = gameCharacter.Input.Move.normalized;

        velocity.x = speed * moveInput.x;
        velocity.z = speed * moveInput.y;

        yVelocity -= gravity * deltaTime;
        if (yVelocity < terminalVelocity)
        {
            yVelocity = terminalVelocity;
        }
        velocity.y = yVelocity;

        characterController.Move(velocity * deltaTime);

        if ((characterController.collisionFlags) != 0)
        {
            grounded = true;
            yVelocity = 0f;
        }
    }

    private void Jump(bool input)
    {
        if (input && grounded)
        {
            yVelocity = jumpStrength;
            grounded = false;
        }
    }
}
