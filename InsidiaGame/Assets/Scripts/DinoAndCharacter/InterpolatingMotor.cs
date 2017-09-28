using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script that serves as an incomplete example of how to program movement, but mainly on how to interact with the CharacterInput class and as an example of using interpolation.
/// To use, simply place on the Dino along with the GameCharacter and CharacterInput scripts then tweak the variables to your liking. Requires Move and Jump to be set inside of CharacterInput to do anything.
/// <para>Created by Christian Clark</para>
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterInput))]
public class InterpolatingMotor : MonoBehaviour {

    //Movement control variables.
    public float speed = 5f;
    public float turnSpeed = 720f;
    public float jumpStrength = 10f;
    public float yVelocity = 0f;
    public float gravity = 10f;
    public bool grounded = false;
    public float terminalVelocity = -20;

    //Private caching variables.
    private CharacterController _characterController;
    private CharacterInput _characterInput;

    //Keep track of the Coroutine that's running for movement.
    private Coroutine _movementCoroutine;
    private const int MOVEMENT_UPDATES_PER_SECOND = 20;

    // A few variables needed in order to make interpolation work.
    private float _lastMovementUpdate = 0f;
    private float _nextMovementUpdate = 0f;
    private Vector3 _lastPosition = Vector3.zero;
    private Quaternion _lastRotiation = Quaternion.identity;
    private Vector3 _nextPosition = Vector3.zero;
    private Quaternion _nextRotation = Quaternion.identity;
    private Vector3 _lastInterpolatedPosition = Vector3.zero;
    private Quaternion _lastInterpolatedRotation = Quaternion.identity;

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

    //Cache references to the CharacterController component and the gameCharacter script.
    public void Awake()
    {
        //Due to the [RequireComponent] attributes on the class itself, it can be garenteed that this code will work (as long as it's set up through Unity's editor).
        _characterController = GetComponent<CharacterController>();
        _characterInput = GetComponent<CharacterInput>();
    }

    //Runs after Awake, but before Start.
    public void OnEnable()
    {
        _movementCoroutine = StartCoroutine(MovementUpdate());

        //Subscribe to the Jump input's delegate
        _characterInput.Jump.OnChange += OnJump;
    }

    public void OnDisable()
    {
        StopCoroutine(_movementCoroutine);

        //Make sure to unsubscribe or else you can cause the program to crash.
        _characterInput.Jump.OnChange -= OnJump;
    }

    //Handles timing of the call to the Move() function and setting up the interpolation.
    IEnumerator MovementUpdate()
    {
        //Initalization
        const float waitPeriod = 1f / MOVEMENT_UPDATES_PER_SECOND;
        _lastMovementUpdate = Time.time;
        yield return new WaitForSeconds(waitPeriod); //Wait a bit in order to set up delta time.

        while (true)
        {
            //Update tracking variables
            _lastPosition = transform.localPosition;
            _lastRotiation = transform.localRotation;

            //Do the movement
            Move(Time.time - _lastMovementUpdate);

            //Keep track of where we ended up and use that as our goal position.
            _nextPosition = transform.localPosition;
            _nextRotation = transform.localRotation;

            //Reset our current position to where we need to be starting from.
            transform.localPosition = _lastInterpolatedPosition = _lastPosition;
            transform.localRotation = _lastInterpolatedRotation = _lastRotiation;

            //Set up the timing for the interpolation
            _lastMovementUpdate = Time.time;
            _nextMovementUpdate = Time.time + waitPeriod;

            yield return new WaitUntil(DoInterpolation);
        }
    }

    //Does the interpolation and returns true when it's done.
    //The last place it interpolated to is where the MovementUpdate() code picks up from.
    //Will stop interpolation if the gameCharacter is suddenly moved.
    private bool DoInterpolation()
    {
        //Cancel the interpolation if our position/rotation was changed by an outside script.
        if ((interpolatePosition && transform.localPosition != _lastInterpolatedPosition) || (interpolateRotation && transform.localRotation != _lastInterpolatedRotation))
            return true;

        //Calculate how long the interpolation needs to last for.
        float periodLength = _nextMovementUpdate - _lastMovementUpdate;
        //And how far along we are in it.
        float progress = Time.time - _lastMovementUpdate;

        //Check to make sure we're not on our first time through the interpolation.
        if (progress == 0)
            return false;

        //Calculate the fractional amount (between 0 and 1) of how far along we are in the interpolation.
        float t = _lastMovementUpdate / periodLength;

        //Debug.Log(lastMovementUpdate + " " + Time.time + " " + nextMovementUpdate + " " + t + " " + periodLength);
        if (interpolatePosition)
            transform.localPosition = Vector3.Lerp(_lastPosition, _nextPosition, t);
        if (interpolateRotation)
            transform.localRotation = Quaternion.Slerp(_lastRotiation, _nextRotation, t);

        _lastInterpolatedPosition = transform.localPosition;
        _lastInterpolatedRotation = transform.localRotation;

        return (t >= 1f);
    }

    //The function subscribed to the OnMovementUpdate of the gameCharacter.
    //It is given both the gameCharacter that sent the event (just in case you don't already have this info)
    //and the amount of time that has passed since the last call of the event (works just like Time.deltaTime, but only for this event).
    private void Move(float deltaTime)
    {
        // Fetch the input values from the game character //
        
        //What is our current move input?
        Vector3 moveInput = new Vector3(_characterInput.Move.Value.x, 0, _characterInput.Move.Value.y);
        moveInput.Normalize();

        // Apply the inputs //

        //Calculate the velocity we need to be moving at using the move input.
        Vector3 velocity = moveInput * speed;

        //Rotate to face the input.
        if (moveInput != Vector3.zero)
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(moveInput, transform.up), turnSpeed * deltaTime);

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
        _characterController.Move(velocity * deltaTime);

        // Resolve movement collisions //

        //Check to see if we collided with the ground.
        if ((_characterController.collisionFlags & CollisionFlags.Below) != 0)
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
    private void OnJump(CharacterInput charInput, bool input)
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
