using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This tells an AI character how to navigate a NavMesh using the same kind of movement system that a player uses. (It goes through CharacterInput.) At the moment it requires a CharacterController so that it can test to make sure it's on the ground.
/// <para>If the AI character gets knocked off course or bumped off a ledge, it will automatically repath to its destination.</para>
/// <para>To use: Place on any AI controlled character that needs to move. In another script, set <see cref="Goal"/> to wherever in the Scene you want it to go. It may not work if there is no valid path it can take.</para>
/// <para>It can follow both OffMeshLinks and the newer NavMeshLinks up simple jumps and off of ledges (based on the direction of the link). To get more fancy behaviour, put a <see cref="CustomNavLinkManager"/> script on a GameObject with one or more <see cref="UnityEngine.AI.NavMeshLink"/>s on it and set that up.</para>
/// <para>Created by Christian Clark</para>
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterInput))]
[RequireComponent(typeof(CharacterController))]
public class AICharMovement : MonoBehaviour {

    private const float AI_UPDATE_FREQUENCY = 30f;
    private Coroutine _coroutineAIUpdate = null;

    private const float LATE_UPDATE_FREQ = 20f;

    private NavMeshAgent _agent;
    public NavMeshAgent Agent { get { return _agent; } }

    private bool _followingLink = false;
    private Coroutine _coroutineFollowLink = null;

    private bool _hasNextGoal = false;
    private Vector3 _nextGoal = Vector3.zero;

    private CharacterController _charController;
    private CharacterInput _charInput;

    private Vector3 _goal = Vector3.zero;
    /// <summary>
    /// Set this to make the AI character go towards a point. It will respect the stopping distance set on the NavMeshAgent.
    /// </summary>
    public Vector3 Goal
    {
        get { return _goal; }
        set
        {
            if (!_followingLink)
            {
                _goal = value;
                _agent.destination = _goal;
            }
            else
            {
                _nextGoal = value;
                _hasNextGoal = true;
            }
        }
    }

    [HideInInspector]
    public Vector3 linkFollowGoal;

    private float _lastLateUpdate = 0f;
    private Vector3 _lastPosition = Vector3.zero;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.autoTraverseOffMeshLink = false;
        _agent.autoRepath = true;

        _charController = GetComponent<CharacterController>();
        _charInput = GetComponent<CharacterInput>();
    }

    private void OnEnable()
    {
        //I made a utility coroutine to call a function a certain amount of times per second.
        _coroutineAIUpdate = StartCoroutine(this.UpdateCoroutine(AI_UPDATE_FREQUENCY, AIUpdate));

        //Tell our NavMeshAgent to set itself to where we currently are and not to move from that spot.
        _goal = transform.position;
        _agent.destination = _goal;
        _lastPosition = transform.position;
    }

    private void OnDisable()
    {
        StopCoroutine(_coroutineAIUpdate);
        if (_coroutineFollowLink != null)
        {
            StopCoroutine(_coroutineFollowLink);
        }
    }

    //Called by a coroutine to run less frequently than the game's FPS.
    private void AIUpdate () {

        //Are we starting a link?
        if (_agent.isOnOffMeshLink && !_followingLink)
        {
            Func<IEnumerator> followDelegate = null;

            //Check to see if there's a custom link behaviour to use.
            if (_agent.navMeshOwner is NavMeshLink)
            {
                NavMeshLink link = _agent.navMeshOwner as NavMeshLink;
                CustomNavLinkManager customLink = link.GetComponent<CustomNavLinkManager>();

                if (customLink != null)
                {
                    CustomNavLinkBehaviour behaviour = customLink.GetBehaviour(link);
                    if (behaviour != null)
                        //Some C# lambda wizardry to create a function that meets the Func<IEnumerator> signature. 
                        followDelegate = (() => behaviour.FollowLink(this));
                }
            }

            //Fall back to the two default link behaviours.
            if (followDelegate == null) {
                bool isRising = (_agent.currentOffMeshLinkData.startPos.y <= _agent.currentOffMeshLinkData.endPos.y);
                if (isRising)
                {
                    followDelegate = FollowStepJumpLink;
                }
                else
                {
                    followDelegate = FollowLedgeFallLink;
                }
            }

            //Start the coroutine! It will control navigation variables and Input while we follow the link.
            _coroutineFollowLink = StartCoroutine(FollowNavMeshLink(followDelegate));
        }

        //Don't allow repathing while following a link, it could have disaterous consequences.
        if (!_followingLink && _hasNextGoal)
        {
            _hasNextGoal = false;
            _agent.destination = _nextGoal;
            _goal = _nextGoal;
        }

        //The recommended way to get the move input from a NavMeshAgent.
        Vector3 moveInput = _agent.desiredVelocity;

        //If we're not auto braking, then lets still stop a short distance away to avoid jittering.
        if (!_agent.autoBraking && GetHorizontalDistance(transform.position, Goal) <= _agent.radius / 2f)
            moveInput = Vector3.zero;

        //If we're following a link, go towards the interim Goal set by the link behaviour.
        if (_followingLink)
        {
            if (!IsAtLinkFollowGoal())
                moveInput = linkFollowGoal - transform.position;
            else
                moveInput = Vector3.zero;
        }

        moveInput.y = 0;
        moveInput.Normalize();

        moveInput *= (_agent.desiredVelocity.magnitude / _agent.speed);

        _charInput.Move.Value = new Vector2(moveInput.x, moveInput.z);
   }

    private void LateUpdate()
    {
        //Needed to keep the NavAgent correcty in regards to the AI character.
        _agent.velocity = _lastPosition - transform.position;
        _lastPosition = transform.position;

        //There's no coroutine yeild instruction for something to happen in this step, so one is improvised. (This is about much work as a Coroutine does to check if it can run anyways.)
        const float WAIT_PERIOD = 1f / LATE_UPDATE_FREQ;
        if (Time.time - _lastLateUpdate > WAIT_PERIOD)
        {
            _lastLateUpdate = Time.time;

            //Make sure we haven't gotten knocked on to the wrong platform via some outside force.
            UpdateAndCheckNavAgentPosition();
        }

        //Debug draw the path the AI Character is taking! Only runs in the editor (not on standalone game build).
#if UNITY_EDITOR
        for (int i = 0; i < _agent.path.corners.Length; i++)
        {
            if (i == 0)
            {
                Debug.DrawLine(transform.position, _agent.path.corners[i], Color.red);
            }
            else
            {
                Debug.DrawLine(_agent.path.corners[i - 1], _agent.path.corners[i], Color.blue);
            }
        }

        if (_followingLink)
        {
            Debug.DrawRay(linkFollowGoal, Vector3.up, (IsAtLinkFollowGoal()) ? Color.green : Color.black);
        }
#endif
    }

    //Disallow AI characters to stand on eachother (not perfect).
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Check to see if we're standing on another character controller.
        if (hit.collider is CharacterController && hit.moveDirection.y < 0 && Vector3.Angle(Vector3.up, hit.normal) < 90f)
        {
            //Make the AI charcter slide a bit off the one it's standing on.
            Vector3 correctionVector = hit.normal;
            correctionVector.y = 0;
            correctionVector *= 0.05f;
            transform.position += correctionVector;
        }
    }

    private void StartLink()
    {
        //May need extra stuff to happen in here at some point.
        _followingLink = true;
    }

    private IEnumerator FollowNavMeshLink(Func<IEnumerator> followDelegate)
    {
        StartLink();
        //You can yield another IEnumerator and that becomes the new Coroutine function until its done.
        yield return followDelegate();
        EndLink();
    }

    private void EndLink()
    {
        _followingLink = false;
        //No need to call StopCoroutine since its stopping soon after this is called anyways.
        _coroutineFollowLink = null;
        
        UpdateAndCheckNavAgentPosition();
    }

    private bool UpdateAndCheckNavAgentPosition()
    {
        _agent.nextPosition = transform.position;

        bool positionCorrect = GetHorizontalDistance(transform.position, _agent.nextPosition) <= _agent.radius && Mathf.Abs(transform.position.y - _agent.nextPosition.y) <= (_agent.height / 2f);

        if (!_followingLink && IsTouchingGround() && !positionCorrect)
        {
            RetryNavigation();
            return true;
        }

        return false;
    }

    //Reset our navigation variables and try again!
    private void RetryNavigation()
    {
        //print("Retry Navigation activated!");
        if (_coroutineFollowLink != null)
        {
            _agent.CompleteOffMeshLink();
            StopCoroutine(_coroutineFollowLink);
        }
        _followingLink = false;

        _agent.Warp(transform.position);
        _agent.nextPosition = transform.position;

        _agent.destination = _goal;
        _agent.isStopped = false;
    }

    private IEnumerator FollowStepJumpLink()
    {
        //Set our goal to the end of the link
        linkFollowGoal = _agent.currentOffMeshLinkData.endPos;

        //Wait until we touch ground then JUMP!
        //WaitUntil is a magical thing. What's inside that function is a anyonmonus function aka "lambda" function.
        yield return new WaitUntil(() => IsTouchingGround());
        _charInput.Jump.Value = true;
        yield return new WaitForSeconds(0.1f);
        _charInput.Jump.Value = false;

        //Wait until we get a bit away from the start to let another AI character on the link. 
        //CompleteOffMeshLink main purpose is to tell the NavMeshAgent that we're now at the end of a link, but finishing a link is handled by the code in this script moreso than by the NavMeshAgent.
        yield return new WaitUntil(() => GetHorizontalDistance(transform.position, _agent.currentOffMeshLinkData.startPos) >= _agent.radius);
        _agent.CompleteOffMeshLink();

        //Wait until we either touch ground again or reach the end before we're done with the link.
        yield return new WaitUntil(() => (IsTouchingGround() || IsAtLinkFollowGoal()));   
    }

    private IEnumerator FollowLedgeFallLink()
    {
        //Go to the end.
        linkFollowGoal = _agent.currentOffMeshLinkData.endPos;
        float startY = _agent.currentOffMeshLinkData.startPos.y + _agent.height / 2f;

        //Free up the link immeadately.
        _agent.CompleteOffMeshLink();
        
        //Make sure we've dropped off the ledge before we return to normal navigation.
        yield return new WaitUntil(() => transform.position.y + NavMesh.GetSettingsByID(_agent.agentTypeID).agentClimb < startY);
    }
    
    public bool IsAtLinkFollowGoal()
    {
        return IsAtLinkFollowGoal(_agent.radius);
    }

    public bool IsAtLinkFollowGoal(float radius)
    {
        return GetHorizontalDistance(transform.position, linkFollowGoal) <= radius;
    }

/*#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Following Link: " + _followingLink);
        GUILayout.Label("Navmesh: " + _agent.navMeshOwner);
        GUILayout.Label("Next Goal " + _hasNextGoal);
        GUILayout.Label("Bound to Mesh?: " + _agent.isOnNavMesh);
        GUILayout.Label("Destination: " + _agent.destination);
        GUILayout.Label("Path Status:" + _agent.pathStatus);
        GUILayout.Label("Agent Stopped?: " + _agent.isStopped);
        GUILayout.EndVertical();
    }
#endif*/

    //Will be updated to not use CharacterController if possible.
    public bool IsTouchingGround()
    {
        return (_charController.collisionFlags & CollisionFlags.Below) != 0;
    }

    //Utility Function to get the horizontal distance between two points.
    public float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        float z = a.z - b.z;
        return Mathf.Sqrt(x * x + z * z);
    }
}
