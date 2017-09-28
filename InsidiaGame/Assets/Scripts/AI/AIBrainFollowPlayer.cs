using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AICharMovement))]
public class AIBrainFollowPlayer : MonoBehaviour {

    private AICharMovement _aiChar;

    public float scanRadius = 10f;
    private const float SCAN_FOR_PLAYER_UPS = 5f;
    private Coroutine _coroutineScan;

    public Transform target;
    private const float FOLLOW_UPS = 20f;

	// Use this for initialization
	void Awake () {
        _aiChar = GetComponent<AICharMovement>();
	}

    private void OnEnable()
    {
        target = null;
        _coroutineScan = StartCoroutine(this.UpdateCoroutine(SCAN_FOR_PLAYER_UPS, ScanUpdate));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void ScanUpdate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, scanRadius);
        foreach (var col in cols)
        {
            if (col.CompareTag("Player"))
            {
                target = col.transform;
                StopCoroutine(_coroutineScan);
                StartCoroutine(this.UpdateCoroutine(FOLLOW_UPS, FollowUpdate));
            }
        }
	}

    void FollowUpdate()
    {
        _aiChar.Goal = target.position;
    }
}
