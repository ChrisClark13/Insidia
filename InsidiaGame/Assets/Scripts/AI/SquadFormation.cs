using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Squad",menuName = "ArchTeam/AI Formation")]
public class SquadFormation : ScriptableObject {
    
    [System.Serializable]
    public class PositionEntry
    {
        public Vector3[] positions;
    }

    public PositionEntry[] positionsByPriority;
    public float maxVerticalVariance = 3f;
    public float maxNavMeshVariance = 1f;
}
