using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AICharMovement))]
public class AIBrainWander : MonoBehaviour {

    private AICharMovement _aiChar;
    private const float UPDATES_PER_SECOND = 10f;

	// Use this for initialization
	void Awake () {
        _aiChar = GetComponent<AICharMovement>();
	}

    private void OnEnable()
    {
        StartCoroutine(this.UpdateCoroutine(UPDATES_PER_SECOND, UpdateWander));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        _aiChar.Goal = AIWanderPoints.Points[Random.Range(0, AIWanderPoints.Points.Length - 1)].position;
    }

    // Update is called once per frame
    void UpdateWander () {
        if (Vector3.Distance(_aiChar.transform.position, _aiChar.Goal) <= _aiChar.Agent.stoppingDistance)
        {
            _aiChar.Goal = AIWanderPoints.Points[Random.Range(0, AIWanderPoints.Points.Length - 1)].position;
        }
	}
}
