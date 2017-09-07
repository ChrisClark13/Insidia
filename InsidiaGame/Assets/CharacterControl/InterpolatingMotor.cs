using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script exists both as an example of how to use gameCharacter.Input and to serve as a simple placeholder movement script.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(GameCharacter))]
public class InterpolatingMotor : MonoBehaviour {

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

    //Keep track of the Coroutine that's running for movement.
    private Coroutine movementCoroutine;
    private const int MOVEMENT_UPDATES_PER_SECOND = 20;

    // A few variables needed in order to make interpolation work.
    private float lastMovementUpdate = 0f;
    private float nextMovementUpdate = 0f;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotiation = Quaternion.identity;
    private Vector3 nextPosition = Vector3.zero;
    private Quaternion nextRotation = Quaternion.identity;
    private Vector3 lastInterpolatedPosition = Vector3.zero;
    private Quaternion lastInterpolatedRotation = Quaternion.identity;

    public void Awake()
    {
        //Due to the [RequireComponent] attributes on the class itself, it can be garuenteed that this code will work (as long as it's set up through Unity's editor).
        characterController = GetComponent<CharacterController>();
        gameCharacter = GetComponent<GameCharacter>();
    }

    //Runs after Awake, but before Start.
    public void OnEnable()
    {
        movementCoroutine = StartCoroutine(MovementUpdate());

        //The input changed event provided for each input in gameCharacter.Input. (See CharacterInputState for how it works.)
        gameCharacter.Input.Jump.OnChange += OnJump;
    }

    public void OnDisable()
    {
        StopCoroutine(movementCoroutine);

        //Make sure to unsubcribe your functions when you no longer want their code to run.
        //Otherwise, the code will run even when your script is disabled.
        gameCharacter.Input.Jump.OnChange -= OnJump;

        //!! Also make sure to do this otherwise you can crash the game! !!
    }

    /// <summary>
    /// Disable if the position of the character needs to be controlled by something other than a script running via OnMovementUpdate.
    /// </summary>
    [Tooltip("Disable if the position of the character needs to be controlled by something other than a script running via OnMovementUpdate.")]
    public bool interpolatePosition = true;
    /// <summary>
    /// Disable if the rotation of the character needs to be controlled by something other than a script running via OnMovementUpdate.
    /// </summary>
    [Tooltip("Disable if the rotation of the character needs to be controlled by something other than a script running via OnMovementUpdate.")]
    public bool interpolateRotation = true;

    IEnumerator MovementUpdate()
    {
        //Initalization
        const float waitPeriod = 1f / MOVEMENT_UPDATES_PER_SECOND;
        lastMovementUpdate = Time.time;
        yield return new WaitForSeconds(waitPeriod); //Wait a bit in order to set up delta time.

        while (true)
        {
            //Update tracking variables
            lastPosition = transform.localPosition;
            lastRotiation = transform.localRotation;

            //Do the movement
            Move(Time.time - lastMovementUpdate);

            //Keep track of where we ended up and use that as our goal position.
            nextPosition = transform.localPosition;
            nextRotation = transform.localRotation;

            //Reset our current position to where we need to be starting from.
            transform.localPosition = lastInterpolatedPosition = lastPosition;
            transform.localRotation = lastInterpolatedRotation = lastRotiation;

            //Set up the timing for the interpolation
            lastMovementUpdate = Time.time;
            nextMovementUpdate = Time.time + waitPeriod;

            yield return new WaitUntil(DoInterpolation);
        }
    }

    //Does the interpolation and returns true when it's done.
    //The last place it interpolated to is where the MovementUpdate() code picks up from.
    //Will stop interpolation if the gameCharacter is suddenly moved.
    private bool DoInterpolation()
    {
        //Cancel the interpolation if our position/rotation was changed by an outside script.
        if ((interpolatePosition && transform.localPosition != lastInterpolatedPosition) || (interpolateRotation && transform.localRotation != lastInterpolatedRotation))
            return true;

        //Calculate how long the interpolation needs to last for.
        float periodLength = nextMovementUpdate - lastMovementUpdate;
        //And how far along we are in it.
        float progress = Time.time - lastMovementUpdate;

        //Check to make sure we're not on our first time through the interpolation.
        if (progress == 0)
            return false;

        //Calculate the fractional amount (between 0 and 1) of how far along we are in the interpolation.
        float t = lastMovementUpdate / periodLength;

        //Debug.Log(lastMovementUpdate + " " + Time.time + " " + nextMovementUpdate + " " + t + " " + periodLength);
        if (interpolatePosition)
            transform.localPosition = Vector3.Lerp(lastPosition, nextPosition, t);
        if (interpolateRotation)
            transform.localRotation = Quaternion.Slerp(lastRotiation, nextRotation, t);

        lastInterpolatedPosition = transform.localPosition;
        lastInterpolatedRotation = transform.localRotation;

        return (t >= 1f);
    }

    //The function subscribed to the OnMovementUpdate of the gameCharacter.
    //It is given both the gameCharacter that sent the event (just in case you don't already have this info)
    //and the amount of time that has passed since the last call of the event (works just like Time.deltaTime, but only for this event).
    private void Move(float deltaTime)
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

        //Rotate according to the aim.x input along the Y-axis.
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
