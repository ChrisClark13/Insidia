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
    public CharacterInputState Input { get { return _input; } }

    /// <summary>
    /// A delegate for functionst that are expensive and/or don't need to be run very often. Use instead of Update(). Runs 10 times a second.
    /// </summary>
    public Action<GameCharacter, float> OnSlowUpdate;
    /// <summary>
    /// A delegate for functions that are cheap and/or need to be run very often. Use instead of Update(). Runs 40 times a second.
    /// </summary>
    public Action<GameCharacter, float> OnFastUpdate;
    /// <summary>
    /// A delegate for functions that move the GameCharacter. Uses interpolation to smooth out the frames inbetween updates. Runs 20 times a second.
    /// </summary>
    public Action<GameCharacter, float> OnMovementUpdate;

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
            if (OnSlowUpdate != null) OnSlowUpdate(this, Time.time - last);
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
            if (OnFastUpdate != null) OnFastUpdate(this, Time.time - last);
            last = Time.time;
        }
    }

    // A load of variables needed in order to make interpolation work.
    private float lastMovementUpdate = 0f;
    private float nextMovementUpdate = 0f;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotiation = Quaternion.identity;
    private Vector3 nextPosition = Vector3.zero;
    private Quaternion nextRotation = Quaternion.identity;

    /// <summary>
    /// Disable if the rotation of the character needs to be controlled by something other than a script running via OnMovementUpdate.
    /// </summary>
    [Tooltip("Disable if the rotation of the character needs to be controlled by something other than a script running via OnMovementUpdate.")]
    public bool interpolateRotation = true;

    IEnumerator MovementUpdate()
    {
        const float waitPeriod = 1f / MOVEMENT_UPDATES_PER_SECOND;
        lastMovementUpdate = Time.time;
        yield return new WaitForSeconds(waitPeriod);

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

                yield return new WaitUntil(IsInterpolationFinished);
            }
            else
            {
                lastMovementUpdate = Time.time;
                nextMovementUpdate = Time.time + waitPeriod;

                yield return new WaitForSeconds(waitPeriod);
            }
        }
    }

    private bool IsInterpolationFinished()
    {
        float periodLength = nextMovementUpdate - lastMovementUpdate;
        float t = Mathf.Clamp01((Time.time - lastMovementUpdate) / periodLength);

        //Debug.Log(lastMovementUpdate + " " + Time.time + " " + nextMovementUpdate + " " + t + " " + periodLength);

        transform.localPosition = Vector3.Lerp(lastPosition, nextPosition, t);
        transform.localRotation = Quaternion.Slerp(lastRotiation, nextRotation, t);

        return (t >= 1f);
    }
}
