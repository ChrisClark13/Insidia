using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ArchTeam/AI Aggro Profile")]
public class AIAggroProfile : ScriptableObject
{
    [System.Serializable]
    public class MultEntry
    {
        public string tag = "Untagged";
        public float multiplier = 1f;
    }

    [Header("Base Settings")]
    [Tooltip("When the AI learns of a new target, this is the initial amount of aggro that is assigned.")]
    [Label("Initial (In Range)")]
    public float initialInRange = 100;
    [Tooltip("If something attacks the AI (or a friend) and that thing is out of range, this inital value is instead used.")]
    [Label("Initial (Out of Range)")]
    public float initialOutOfRange = 50;
    [Tooltip("If the AI has a target with an aggro amount above this threshold, it will try to attack it.")]
    public float attackThreshold = 20;
    [Tooltip("This the cap on the amount of aggro that can be generated towards a target.")]
    public float maxAggro = 500;
    [Tooltip("When determining who to attack, this number will be multiplied by the distance to each target and subtracted from the aggro"
        +" score in the calucations. This makes distant targets less of a priorty than close ones. Not effected by type multipliers.")]
    public float checkLossPerMeter = 5f;
    [Tooltip("How many points of aggro are gained for each point of damage dealt?")]
    public float aggroPerDamageMultiplier = 0.1f;
    [Tooltip("How much the aggro changes can vary randomly measured in percentage. A higher value means an AI that decides what it wants to attack nearly randomly. (Does not effect Initial and Loss Per Meter)")]
    [Range(0f, 2f)]
    public float randomVariance = 0.1f;

    [Header("Tag Settings")]
    [Tooltip("When checking targets to see which has the highest aggro, multiply in the matching multiplier according to that targets tag. This makes some tags take priority over others.")]
    public List<MultEntry> targetTagMultipliers = new List<MultEntry>();
    [Tooltip("If a target has a tag that's not in our list, use this multiplier as a fallback.")]
    public float defaultTargetMultiplier = 0.25f;

    [Header("Squad Settings")]
    [Tooltip("If we're out of range of our Squad, then this is how fast we lose aggro on everything at minimum.")]
    public float selfOutOfRangeDecayMin = 15f;
    [Tooltip("If we're out of range of our Squad, then this is how fast we lose aggro on everything at maximum.")]
    public float selfOutOfRangeDecayMax = 100f;
    [Tooltip("How far out of range we have to be in order to get the max decay rate")]
    public float selfMaxDecayDistance = 30f;

    [Header("No Squad Settings")]
    [Tooltip("If we're not in a squad, then use this as our range.")]
    public float soloRange = 10f;
    [Tooltip("IF we're not in a squad, this is how fast our aggro on everything decays every second.")]
    public float soloDecayRate = 5f;

    [Header("Out of Range Settings", order = 0)]
    [Header("(Solo Range is used instead if the AI is not in a Squad)", order = 1)]
    [Tooltip("If the target is outside the squad's range, then lose at least this much aggro per second based on how far away it is.")]
    public float outOfRangeDecayMin = 10;
    [Tooltip("If the target is outside the squad's range, then lose at most this much aggro per second based on how far away it is.")]
    public float outOfRangeDecayMax = 50;
    [Tooltip("How far the target has to be outside of the range to get the max decay rate")]
    public float targetMaxDecayDistance = 20f;

    [Header("Damage Event Settings")]
    [Label("Target -> Self")]
    public float targetAttackedSelf = 50;
    [Label("Self -> Target")]
    public float selfAttackedTarget = 10;
    [Label("Target -> Friend")]
    public float targetAttackedFriend = 10;
    [Label("Friend -> Target")]
    public float friendAttackedTarget = -10;
    [Label("Target -> Leader/Player")]
    public float targetAttackedLeader = 20;
    [Label("Leader/Player -> Target")]
    public float leaderAttackedTarget = 10;

    [Header("Death Event Settings")]
    public float targetKilledFriend = 20;
    public float targetKilledLeader = 40;

    public float GetMultiplier(GameObject target)
    {
        float mult = defaultTargetMultiplier;

        foreach (var entry in targetTagMultipliers)
        {
            if (entry.tag == target.tag)
            {
                mult = entry.multiplier;
                break;
            }
        }

        return mult;
    }

    public float CalcAggro(float baseAggro, GameObject target, bool applyVariance = true)
    {
        float variance = (applyVariance) ? 1f + (Random.Range(0, randomVariance) - randomVariance / 2f) : 1f;
        return Mathf.Sign(baseAggro) * Mathf.Max(0, Mathf.Abs(baseAggro) * variance);
    }

    public float CalcDamageAggro(float baseAggro, GameObject target, float damageAmount)
    {
        return CalcAggro(baseAggro + (damageAmount * aggroPerDamageMultiplier), target);
    }
}
