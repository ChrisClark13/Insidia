using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AISquad_CC : MonoBehaviour {

    public SquadFormation formation;
    public List<AICharMovement> units;
	
	// Update is called once per frame
	void Start () {
        StartCoroutine(this.UpdateCoroutine(10f, SetUnitGoalsToPositions));
    }

    public void SetUnitGoalsToPositions()
    {
        if (units != null && units.Count == 0)
            return;

        List<AICharMovement> unitsToAssign = new List<AICharMovement>(units);

        //Check all positions against our current position to make sure they are valid.

        //Get a spot on the ground to test the formation positions against, otherwise just use our current position.
        Vector3 testPosition = transform.position;
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit))
        {
            //Test to see if the ground is flat here?
            testPosition = groundHit.point;
        }

        //2D list go! Use it to store the results of the position validations.
        List<List<Vector3>> validPositions = new List<List<Vector3>>(formation.positionsByPriority.Length);
        int validCountsCount = 0;
        foreach (var positionRow in formation.positionsByPriority)
        {
            bool validPosRowAdded = false;
            foreach (Vector3 pos in positionRow.positions)
            {
                //First, make sure the point isn't too low or too high from our current position.
                RaycastHit raycastHit;
                Vector3 point = testPosition + transform.TransformVector(pos);
                point.y += formation.maxVerticalVariance / 2f;
                if (Physics.Raycast(point, Vector3.down, out raycastHit, formation.maxVerticalVariance))
                {
                    //Then make sure there's a position on the NavMesh that can correspond to that position.
                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(raycastHit.point, out navMeshHit, formation.maxNavMeshVariance, units[0].Agent.areaMask))
                    {
                        //Fill out each row in the 2D list as we get to it.
                        if (validPositions.Count < validCountsCount + 1)
                        {
                            validPositions.Add(new List<Vector3>(formation.positionsByPriority[validCountsCount].positions.Length));
                            validPosRowAdded = true;
                        }

                        validPositions[validCountsCount].Add(navMeshHit.position);
                    }
                }
            }

            if (validPosRowAdded)
                validCountsCount++;
        }

        //If there's no valid positions, don't attempt to assign anything to them!
        if (validPositions.Count == 0)
            return;

        //Go through the positions in order of priority and assign units to them based on the closest unit to each point.
        foreach (var positionRow in validPositions)
        {
            //If we run out of units to assign, we're done with this function.
            if (unitsToAssign.Count == 0)
                return;

            //Pair off the closest AI unit and position until we run out of either.
            List<Vector3> avaliblePositions = new List<Vector3>(positionRow);
            while (avaliblePositions.Count > 0 && unitsToAssign.Count > 0)
            {
                Vector3 pos;
                AICharMovement unit;

                FindClosestUnitAndPosition(unitsToAssign, avaliblePositions, out unit, out pos);

                unit.Goal = pos;

                avaliblePositions.Remove(pos);
                unitsToAssign.Remove(unit);
            }

            ////Iterate through the positions.
            //foreach (var pos in positionRow)
            //{
            //    //If we ran out of units to assign, we're done with this entire function.
            //    if (unitsToAssign.Count == 0)
            //        return;

            //    //Find the closest unit to the point
            //    var unit = FindClosestUnitFromList(pos, unitsToAssign);
            //    //Remove it from the list of units to assign.
            //    unitsToAssign.Remove(unit);

            //    //And assign it to the position.
            //    unit.Goal = pos;
            //    print(unit.gameObject.name + " to " + pos);
            //}
        }

        //Fallback case
        //Check to see if we have left over units
        if (unitsToAssign.Count > 0)
        {
            //If we do, assign them round-robin to the lowest priority positions

            //Get how many positions are in the last vaild row.
            int rowCount = validPositions[validCountsCount - 1].Count;

            //Iterate through the leftover units and assign them to the positions.
            for (int i = 0; i < unitsToAssign.Count; i++)
            {    
                unitsToAssign[i].Goal = validPositions[validCountsCount - 1][i % rowCount];
            }
        }
    }

    private AICharMovement FindClosestUnitFromList(Vector3 pos, List<AICharMovement> units)
    {
        AICharMovement closestUnit = null;
        float closestDistance = 0f;

        foreach (var unit in units)
        {
            if (closestUnit == null)
            {
                closestUnit = unit;
                closestDistance = Vector3.Distance(closestUnit.transform.position, pos);
            }
            else
            {
                float newDistance = Vector3.Distance(unit.transform.position, pos);
                if (newDistance < closestDistance)
                {
                    closestUnit = unit;
                    closestDistance = newDistance;
                }
            }
        }

        return closestUnit;
    }

    private void FindClosestUnitAndPosition(List<AICharMovement> units, List<Vector3> positions, out AICharMovement unit, out Vector3 position)
    {
        float minDist = float.PositiveInfinity;
        AICharMovement closestUnit = null;
        Vector3? closestPos = null;

        foreach(var _unit in units)
        {
            foreach(var _pos in positions)
            {
                float dist = (_unit.transform.position - _pos).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closestUnit = _unit;
                    closestPos = _pos;
                }
            }
        }

        unit = closestUnit;
        position = closestPos.GetValueOrDefault();
    }
}
