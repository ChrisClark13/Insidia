using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIClickToMove : MonoBehaviour {

    public AICharMovement aiMovement;
	
	// Update is called once per frame
	void Update () {
        if (Time.time < 0.5f)
            return;

	    if (Input.GetMouseButtonDown(0))
        {
            Vector3? point = FindPointUnderMouse();
            if (point != null)
                aiMovement.Goal = point.Value;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector3? point = FindPointUnderMouse();
            if (point != null)
            {
                aiMovement.transform.position = point.Value;
            }
        }
	}

    private Vector3? FindPointUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray);

        RaycastHit? closestHit = null;

        foreach (var hit in hits)
        {
            if (!hit.transform.IsChildOf(aiMovement.transform))
                if (!closestHit.HasValue)
                    closestHit = hit;
                else if (hit.distance < closestHit.Value.distance)
                    closestHit = hit;
        }

        if (closestHit.HasValue)
            return closestHit.Value.point;
        else
            return null;
    }
}
