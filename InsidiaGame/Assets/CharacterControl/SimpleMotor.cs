using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script exists both as an example of how to use gameCharacter.Input and to serve as a simple placeholder movement script.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(GameCharacter))]
public class SimpleMotor : MonoBehaviour {

    //Movement control variables.
    public float speed = 5f;
    public float sprintSpeed = 10f;
    public float turnSpeed = 0.1f;
    public float jumpStrength = 10f;
    public float yVelocity = 0f;
    public float gravity = 10f;
    public bool grounded = false;
    public float terminalVelocity = -20;

    //Private caching variables.
    private CharacterController characterController;
    private GameCharacter gameCharacter;

    public void Awake()
    {
        //Due to the [RequireComponent] attributes on the class itself, it can be garuenteed that this code will work (as long as it's set up through Unity's editor).
        characterController = GetComponent<CharacterController>();
        gameCharacter = GetComponent<GameCharacter>();
    }

    //Runs after Awake, but before Start.
    public void OnEnable()
    {
        //Subscribe to the delegates we need to in order to make this script work.
        //The movement update event provided by the GameCharacter class. Runs 20 times a second and interpolates between the results automatically.
        //You just need to worry about doing all the movement and rotation using a function subscribed to this event if possible.
        gameCharacter.OnMovementUpdate += Move;
        //The input changed event provided for each input in gameCharacter.Input. (See CharacterInputState for how it works.)
        gameCharacter.Input.Jump.OnChange += OnJump;
    }

    public void OnDisable()
    {
        //Make sure to unsubcribe your functions when you no longer want their code to run.
        //Otherwise, the code will run even when your script is disabled.
        gameCharacter.OnMovementUpdate -= Move;
        gameCharacter.Input.Jump.OnChange -= OnJump;

        //!! Also make sure to do this otherwise you can crash the game! !!
    }

    //The function subscribed to the OnMovementUpdate of the gameCharacter.
    //It is given both the gameCharacter that sent the event (just in case you don't already have this info)
    //and the amount of time that has passed since the last call of the event (works just like Time.deltaTime, but only for this event).
    private void Move(GameCharacter sender, float deltaTime)
    {
        // Fetch the input values from the game character //
        
        //Are we sprinting?
        float _speed = (gameCharacter.Input.Sprint) ? sprintSpeed : speed;
        //What is our current move input?
        Vector2 moveInput = gameCharacter.Input.Move;
        moveInput.Normalize();
        //Alternative way of fetching input.
        Vector2 aimInput = gameCharacter.Input.Aim.Value * turnSpeed;

        // Apply the inputs //

        //Rotate according to the aim input along the Y-axis.
        transform.Rotate(Vector3.up, aimInput.x);

        //Calculate the velocity we need to be moving at using the move input.
        Vector3 velocity = Vector3.zero;
        velocity.x = _speed * moveInput.x;
        velocity.z = _speed * moveInput.y;

        //Rotate the velocity according to our current rotation.
        velocity = transform.localRotation * velocity;

        // Calucate physics //

        //Calcuate y velocity.
        //First apply gravity acceleration. (Which is speed per second so use deltaTime here.)
        yVelocity -= gravity * deltaTime;
        //Make sure we don't fall too fast, both for gameplay reasons (staying in control of the character as it falls)
        //and technical reasons (not clipping through the ground)
        if (yVelocity < terminalVelocity)
            yVelocity = terminalVelocity;
        //Apply the y velocity to the velocity vector.
        velocity.y = yVelocity;

        // Apply physics //

        //Move according to our velocity (which is in speed per second so use deltaTime).
        characterController.Move(velocity * deltaTime);

        // Resolve movement collisions //

        //Check to see if we collided with the ground.
        if ((characterController.collisionFlags & CollisionFlags.Below) != 0)
        {
            //If we collided with the ground, then we're grounded.
            grounded = true;
            //Also set y velocity to zero.
            yVelocity = 0f;
        }
        else
            //otherwise, we're not grounded
            grounded = false;
    }

    //This function is subscribed to gameCharacter.Input.Jump.OnChange, meaning that it is called every time
    //the Jump input changes from off to on or on to off (is pressed or released).
    //This function exists because otherwise the input of the jump button being pressed could be missed (making for laggy controls)
    //and because if this was being checked in Move() the player could jump repeatedly by holding the button down.
    private void OnJump(bool input)
    {
        //Only jump if the Jump input changed from off to on (was pressed) and we're on the ground.
        if (input && grounded)
        {
            //This y velocity change will be applied in the next movement update.
            yVelocity += jumpStrength;
            //Make sure we're not grounded anymore (both to prevent doing multiple jumps inbetween movement updates)
            //and to let other scripts we're no longer on the ground.
            grounded = false;
        }
    }
}
