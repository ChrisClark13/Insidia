using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterInput))]
[RequireComponent(typeof(CharacterController))]
public class AICharMovement : MonoBehaviour {

    private const float AI_UPDATE_FREQUENCY = 20f;
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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.autoTraverseOffMeshLink = false;
        _agent.autoRepath = true;

        _charController = GetComponent<CharacterController>();
        _charInput = GetComponent<CharacterInput>();
        _goal = transform.position;
    }

    private void OnEnable()
    {
        _coroutineAIUpdate = StartCoroutine(this.UpdateCoroutine(1f / AI_UPDATE_FREQUENCY, AIUpdate));
    }

    private void OnDisable()
    {
        StopCoroutine(_coroutineAIUpdate);
        if (_coroutineFollowLink != null)
        {
            StopCoroutine(_coroutineFollowLink);
        }
    }

    private float _followLinkTime = 0f;
    private void AIUpdate () {
        if (_agent.isOnOffMeshLink && !_followingLink)
        {
            Func<IEnumerator> followDelegate = null;
            _followLinkTime = 0f;
            if (_agent.navMeshOwner is NavMeshLink)
            {
                NavMeshLink link = _agent.navMeshOwner as NavMeshLink;
                CustomNavLinkManager customLink = link.GetComponent<CustomNavLinkManager>();

                if (customLink != null)
                {
                    CustomNavLinkBehaviour behaviour = customLink.GetBehaviour(link);
                    if (behaviour != null)
                        followDelegate = (() => behaviour.FollowLink(this));
                }
            }

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

            _coroutineFollowLink = StartCoroutine(FollowNavMeshLink(followDelegate));
        }

        if (!_followingLink && _hasNextGoal)
        {
            _hasNextGoal = false;
            _agent.destination = _nextGoal;
            _goal = _nextGoal;
        }

        Vector3 moveInput = _agent.desiredVelocity;
        
        if (_followingLink)
        {
            if (!IsAtLinkFollowGoal())
                moveInput = linkFollowGoal - transform.position;
            else
                moveInput = Vector3.zero;
        }

        moveInput.y = 0;

        moveInput.Normalize();
        _charInput.Move.Value = new Vector2(moveInput.x, moveInput.z);
   }

    private float _lastLateUpdate = 0f;
    private void LateUpdate()
    {
        const float WAIT_PERIOD = 1f / LATE_UPDATE_FREQ;
        if (Time.time - _lastLateUpdate > WAIT_PERIOD)
        {
            float delta = Time.time - _lastLateUpdate;
            _lastLateUpdate = Time.time;

            TestNavAgentPosition();
        }

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

    private void TestNavAgentPosition()
    {
        _agent.velocity = _charController.velocity;
        _agent.nextPosition = transform.position;

        bool positionCorrect = GetHorizontalDistance(transform.position, _agent.nextPosition) <= _agent.radius && Mathf.Abs(transform.position.y - _agent.nextPosition.y) <= (_charController.height / 2f);

        if (!_followingLink && IsTouchingGround() && !positionCorrect)
            RetryNavigation();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider is CharacterController)
        {
            //print(Vector3.Angle(Vector3.up, hit.normal));
            if (hit.moveDirection.y < 0 && Vector3.Angle(Vector3.up, hit.normal) < 90f)
            {
                Vector3 correctionVector = hit.normal;
                correctionVector.y = 0;
                correctionVector *= 0.05f;
                transform.position += correctionVector;
            }
        }
    }

    private void StartLink()
    {
        _followingLink = true;
        _agent.ActivateCurrentOffMeshLink(false);
    }

    private void EndLink()
    {
        _agent.ActivateCurrentOffMeshLink(true);
        _agent.CompleteOffMeshLink();
        _followingLink = false;
        _coroutineFollowLink = null;
        
        TestNavAgentPosition();
    }

    private IEnumerator FollowNavMeshLink(Func<IEnumerator> followDelegate)
    {
        StartLink();
        yield return followDelegate();
        EndLink();
    }

    private IEnumerator FollowStepJumpLink()
    {
        linkFollowGoal = _agent.currentOffMeshLinkData.endPos;

        yield return new WaitUntil(() => IsTouchingGround());
        _charInput.Jump.Value = true;
        yield return new WaitForSeconds(0.1f);
        _charInput.Jump.Value = false;
        yield return new WaitUntil(() => (IsTouchingGround() || IsAtLinkFollowGoal()));   
    }

    private IEnumerator FollowLedgeFallLink()
    {
        linkFollowGoal = _agent.currentOffMeshLinkData.endPos;
        float startY = _agent.currentOffMeshLinkData.startPos.y + _charController.height/2f;
        yield return new WaitUntil(() => transform.position.y + _charController.stepOffset < startY);
    }

    
    public bool IsAtLinkFollowGoal()
    {
        return IsAtLinkFollowGoal(_charController.radius);
    }

    public bool IsAtLinkFollowGoal(float radius)
    {
        return GetHorizontalDistance(transform.position, linkFollowGoal) <= radius;
    }

#if UNITY_EDITOR
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
#endif

    public bool IsTouchingGround()
    {
        return (_charController.collisionFlags & CollisionFlags.Below) != 0;
    }

    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        float z = a.z - b.z;
        return Mathf.Sqrt(x * x + z * z);
    }

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
}
