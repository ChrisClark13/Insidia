using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacter : MonoBehaviour {

    private const int SLOW_UPDATES_PER_SECOND = 10;
    private const int FAST_UPDATES_PER_SECOND = 40;
    private const int MOVEMENT_UPDATES_PER_SECOND = 20;

    [SerializeField]
    private CharacterInputState _input = new CharacterInputState();
    /// <summary>
    /// Use this variable to check the character's inputs.
    /// <para>You will need to subscribe to an input's delegate if you need to know when it is pressed/released as opposed to just needing whatever value the input happens to be at at the time.</para>
    /// How to get an input: "gameCharacter.Input.[input name];" (Allows for implict conversion using some C# wizardry. Referencing the Value property itself is also possible.)<para></para>
    /// How to set an input: "gamecharacter.Input.[input name].Value = value;"<para></para>
    /// How to subscribe to an input delegate: "gameCharacter.Input.[input name].OnChange += [subscriber function];"<para></para>
    /// How to unsubscribe to an input delegate: "gameCharacter.Input.[input name].OnChange -= [subscriber function];"
    /// </summary>
    public CharacterInputState Input { get { return _input; } }

    /// <summary>
    /// Used for coroutine driven updates.
    /// </summary>
    /// <param name="deltaTime">The amount of time that's passed between each call to this delegate.</param>
    public delegate void OnCoroutineUpdate(GameCharacter sender, float deltaTime);

    /// <summary>
    /// An event for functions that are expensive and/or don't need to be run very often. Use instead of Update(). Runs 10 times a second.
    /// </summary>
    public event OnCoroutineUpdate OnSlowUpdate;
    /// <summary>
    /// An event for functions that are cheap and/or need to be run very often. Use instead of Update(). Runs 40 times a second.
    /// </summary>
    public event OnCoroutineUpdate OnFastUpdate;
    /// <summary>
    /// An event for functions that move the GameCharacter. Uses interpolation of both position and rotation to smooth out the frames inbetween updates. Runs 20 times a second.<para></para>
    /// Primarily the GameCharacter's position and rotation should be changed by subscribers of this event.
    /// </summary>
    public event OnCoroutineUpdate OnMovementUpdate;

    private Coroutine slowUpdate;
    private Coroutine fastUpdate;
    private Coroutine movementUpdate;

    private void OnEnable()
    {
        slowUpdate = StartCoroutine(SlowUpdate());
        fastUpdate = StartCoroutine(FastUpdate());
        movementUpdate = StartCoroutine(MovementUpdate());
    }

    private void OnDisable()
    {
        StopCoroutine(slowUpdate);
        StopCoroutine(fastUpdate);
        StopCoroutine(movementUpdate);
    }

    IEnumerator SlowUpdate()
    {
        const float waitPeriod = 1f / SLOW_UPDATES_PER_SECOND;
        float last = Time.time;
        while (true)
        {
            yield return new WaitForSeconds(waitPeriod);
            if (OnSlowUpdate != null)
                //Calculate delta time. (The amount of time that has passed between calls to this coroutine.)
                OnSlowUpdate(this, Time.time - last);
            last = Time.time;
        }
    }

    IEnumerator FastUpdate()
    {
        const float waitPeriod = 1f / FAST_UPDATES_PER_SECOND;
        float last = Time.time;
        while (true)
        {
            yield return new WaitForSeconds(waitPeriod);
            if (OnFastUpdate != null)
                OnFastUpdate(this, Time.time - last);
            last = Time.time;
        }
    }

    // A few variables needed in order to make interpolation work.
    private float lastMovementUpdate = 0f;
    private float nextMovementUpdate = 0f;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotiation = Quaternion.identity;
    private Vector3 nextPosition = Vector3.zero;
    private Quaternion nextRotation = Quaternion.identity;
    private bool interpolating = false;

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

            if (OnMovementUpdate != null)
            {
                OnMovementUpdate(this, Time.time - lastMovementUpdate);
                nextPosition = transform.localPosition;
                nextRotation = transform.localRotation;

                lastMovementUpdate = Time.time;
                nextMovementUpdate = Time.time + waitPeriod;

                yield return new WaitUntil(DoInterpolation);
            }
            else
            {
                lastMovementUpdate = Time.time;
                nextMovementUpdate = Time.time + waitPeriod;

                yield return new WaitForSeconds(waitPeriod);
            }
        }
    }

    private bool DoInterpolation()
    {
        if (interpolating)
        {
            float periodLength = nextMovementUpdate - lastMovementUpdate;
            //Calculate the fractional amount (between 0 and 1) of how far along we are in the interpolation.
            float t = (Time.time - lastMovementUpdate) / periodLength;

            //Debug.Log(lastMovementUpdate + " " + Time.time + " " + nextMovementUpdate + " " + t + " " + periodLength);
            if (interpolatePosition)
                transform.localPosition = Vector3.Lerp(lastPosition, nextPosition, t);
            if (interpolateRotation)
                transform.localRotation = Quaternion.Slerp(lastRotiation, nextRotation, t);

            return (t >= 1f);
        }
        else
            return true;
    }

    /// <summary>
    /// Use in a script if you need to suddenly set a gameCharacter's position and/or rotation.
    /// On the next frame, interpolation will resume for the gameCharacter.
    /// </summary>
    public void EndInterpolation()
    {
        interpolating = false;
    }
}
