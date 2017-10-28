using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "New Squad", menuName = "ArchTeam/AI Formation")]
public class SquadFormation : ScriptableObject {

    [Serializable]
    public class PositionEntry
    {
        public Vector3[] positions;
    }

    public PositionEntry[] positionsByPriority;
    public float maxVerticalVariance = 3f;
    public float maxNavMeshVariance = 1f;
    public float stoppingDistance = 0.5f;

    public void SetMinionGoalsToPositions(Transform baseTransform, List<Minion> units)
    {
        if (units != null && units.Count == 0)
            return;

        List<AICharMovement> unitsToAssign = new List<AICharMovement>(units.Count);
        units.ForEach(unit => unitsToAssign.Add(unit.GetComponent<AICharMovement>()));

        //Check all positions against our current position to make sure they are valid.
        List<List<Vector3>> validPositions = CalculateValidPositions(baseTransform, unitsToAssign[0].Agent.areaMask);

        //If there's no valid positions, don't attempt to assign anything to them!
        if (validPositions.Count == 0)
            return;

        AssignPoisitions(unitsToAssign, validPositions);
    }

    private void AssignPoisitions(List<AICharMovement> unitsToAssign, List<List<Vector3>> validPositions)
    {
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
        int rowsCount = validPositions.Count;
        if (unitsToAssign.Count > 0)
        {
            //If we do, assign them round-robin to the lowest priority positions

            //Get how many positions are in the last vaild row.
            int positionsInRow = validPositions[rowsCount - 1].Count;

            //Iterate through the leftover units and assign them to the positions.
            for (int i = 0; i < unitsToAssign.Count; i++)
            {
                unitsToAssign[i].Goal = validPositions[rowsCount - 1][i % positionsInRow];
            }
        }
    }

    private List<List<Vector3>> CalculateValidPositions(Transform baseTransform, int areaMask)
    {
        List<List<Vector3>> validPositions = new List<List<Vector3>>(positionsByPriority.Length);
        int validRowsCount = 0;

        //Get a spot on the ground to test the formation positions against, otherwise just use our current position.
        Vector3 testPosition = baseTransform.position;
        RaycastHit groundHit;
        if (Physics.Raycast(baseTransform.position, Vector3.down, out groundHit))
        {
            //Test to see if the ground is flat here?
            testPosition = groundHit.point;
        }

        foreach (var positionRow in positionsByPriority)
        {
            bool validPosRowAdded = false;
            foreach (Vector3 pos in positionRow.positions)
            {
                //First, make sure the point isn't too low or too high from our current position.
                RaycastHit raycastHit;
                Vector3 point = testPosition + baseTransform.TransformVector(pos);
                point.y += maxVerticalVariance / 2f;
                if (Physics.Raycast(point, Vector3.down, out raycastHit, maxVerticalVariance))
                {
                    //Then make sure there's a position on the NavMesh that can correspond to that position.
                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(raycastHit.point, out navMeshHit, maxNavMeshVariance, areaMask))
                    {
                        //Fill out each row in the 2D list as we get to it.
                        if (validPositions.Count < validRowsCount + 1)
                        {
                            validPositions.Add(new List<Vector3>(positionsByPriority[validRowsCount].positions.Length));
                            validPosRowAdded = true;
                        }

                        validPositions[validRowsCount].Add(navMeshHit.position);
                    }
                }
            }

            if (validPosRowAdded)
                validRowsCount++;
        }

        return validPositions;
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

        foreach (var _unit in units)
        {
            foreach (var _pos in positions)
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
