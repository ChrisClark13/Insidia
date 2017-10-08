using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {

    public Vector3 upDirection = Vector3.up;
    public Transform target;
	
	// Update is called once per frame
	void LateUpdate () {
        if (target)
            transform.LookAt(target, upDirection);
	}
}
