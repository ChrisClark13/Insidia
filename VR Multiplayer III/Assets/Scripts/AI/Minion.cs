using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(AICharMovement))]
public class Minion : MonoBehaviour {

    public enum State { None, Follow, Attack, Regroup }
    public State state = State.Follow;
    private State prevState = State.None;
    /// <summary>
    /// Contains a reference to the variables for the aggro calculations.
    /// </summary>
    public AIAggroProfile aggroProfile;
    /// <summary>
    /// Keeps track of who this minion is mad at and how mad they are at that thing.
    /// </summary>
    public Dictionary<GameObject, float> aggroDict = new Dictionary<GameObject, float>();

    public GameObject attackTarget = null;
    public float meleeAttackDistanceBuffer = 1.5f;

    public MinionSquad Squad { get; private set; }
    private AICharMovement _charMovement;

    private float attackWait = 0.5f;
    private float lastAttackTime = 0f;

    public void ChangeSquad(MinionSquad newSquad)
    {
        if (newSquad.minions.Count < newSquad.capacity)
        {
            //Leave current squad code
            if (Squad != null)
            {
                Squad.minions.Remove(this);
                Squad.TargetAdded -= OnSquadTargetAdded;
                //Forget any targets we might be mad at since we just changed squads.
                aggroDict.Clear();

                //join new squad
                Squad = newSquad;
                Squad.minions.Add(this);
                //Subscribe so that we know when new targets are added.
                Squad.TargetAdded += OnSquadTargetAdded;
                //Add in any current targets the new squad to our aggro dictionary.
                Squad.targetSensor.sensedObjects.ForEach(obj => OnSquadTargetAdded(Squad, obj));
            }
            else
            {
                //Intialize self into squad
                Squad = newSquad;
                if (!Squad.minions.Contains(this))
                    Squad.minions.Add(this);

                Squad.TargetAdded += OnSquadTargetAdded;
                Squad.targetSensor.sensedObjects.ForEach(obj => OnSquadTargetAdded(Squad, obj));
            }
        }
    }

    private void AddAggro(GameObject target, float aggroChange)
    {
        aggroDict[target] = Mathf.Clamp(aggroDict[target] + aggroProfile.CalcAggro(aggroChange, target), 0, aggroProfile.maxAggro);
    }

    private void OnSquadTargetAdded(MinionSquad squad, GameObject target)
    {
        if (!aggroDict.ContainsKey(target))
            aggroDict.Add(target, aggroProfile.CalcAggro(aggroProfile.initialInRange, target, false));
    }

    //For when we need to aggro things that aren't currently on our target list.
    private void AddTarget(GameObject target)
    {
        if (aggroDict.ContainsKey(target))
            return;

        float aggro = aggroProfile.initialOutOfRange;

        if (Squad)
        {
            if ((Squad.transform.position - target.transform.position).sqrMagnitude <= Squad.range * Squad.range)
                aggro = aggroProfile.initialInRange;
        }
        else
        {
            if ((gameObject.transform.position - target.transform.position).sqrMagnitude <= aggroProfile.soloRange * aggroProfile.soloRange)
                aggro = aggroProfile.initialInRange;
        }

        aggroDict.Add(target, aggroProfile.CalcAggro(aggro, target, false));
    }

    public void Start()
    {
        _charMovement = GetComponent<AICharMovement>();
        lastAttackTime = -attackWait;
    }

    public void OnEnable()
    {
        StartCoroutine(this.UpdateCoroutine(20f, MinionUpdate));

        NetworkedHealth.HealthChanged += OnHealthChanged;
        NetworkedHealth.Death += OnDeath;
    }

    public void OnDisable()
    {
        StopAllCoroutines();

        NetworkedHealth.HealthChanged -= OnHealthChanged;
        NetworkedHealth.Death -= OnDeath;
    }

    private void OnDestroy()
    {
        if (Squad)
        {
            //This will stop an error from happening.
            Squad.TargetAdded -= OnSquadTargetAdded;
        }
    }

    public void MinionUpdate(float deltaTime)
    {
        if (state != prevState)
        {
            StateChanged(prevState, state);
            prevState = state;
        }

        if (state != State.Regroup)
            AggroUpdate(deltaTime);

        switch (state)
        {
            case State.Attack:
                AttackUpdate();
                break;
            case State.Follow:
                FollowUpdate();
                break;
            case State.Regroup:
            default:
                RegroupUpdate();
                break;
        }
    }

    public void StateChanged(State prevState, State newState)
    {
        if (newState == State.Follow)
        {
            if (Squad)
                _charMovement.Agent.stoppingDistance = Squad.formation.stoppingDistance;
        }
        else if (newState == State.Attack)
        {
            //Adjust stopping distance
            _charMovement.Agent.stoppingDistance = meleeAttackDistanceBuffer;
        }
    }

    private void AggroUpdate(float deltaTime)
    {
        float selfAggroDecay = (Squad) ? 0f : aggroProfile.soloDecayRate;
        float dist = 0f;

        //Check if we're out of range of our squad.
        if (Squad && Vector3.Distance(transform.position, Squad.transform.position) > Squad.range)
            selfAggroDecay = Mathf.Lerp(aggroProfile.selfOutOfRangeDecayMin, aggroProfile.selfOutOfRangeDecayMax, (dist - Squad.range) / aggroProfile.selfMaxDecayDistance);

        //Check if any targets are out of range.
        Vector3 measurePoint = (Squad) ? Squad.transform.position : transform.position;
        float range = (Squad) ? Squad.range : aggroProfile.soloRange;

        //Copy the list of keys and iterate over it (having a copied list means that we can change things in the original dictionary without C# getting mad at us).
        foreach (var target in new List<GameObject>(aggroDict.Keys))
        {
            float targetAggroDecay = 0f;
            dist = Vector3.Distance(target.transform.position, measurePoint);
            if (dist > range)
            {
                targetAggroDecay = Mathf.Lerp(aggroProfile.outOfRangeDecayMin, aggroProfile.outOfRangeDecayMax, (dist - range) / aggroProfile.targetMaxDecayDistance);
                //If the target has 0 aggro and is out of range, then remove it from the dictionary.
                if (aggroDict[target] <= 0)
                {
                    aggroDict.Remove(target);
                    //Go straight to the next target in the list.
                    continue;
                }
                else if (!target.activeInHierarchy)
                {
                    aggroDict.Remove(target);
                }
                else
                {
                    //Don't target dead things
                    var targetHealth = target.GetComponent<NetworkedHealth>();
                    if (targetHealth.Health <= 0)
                        aggroDict.Remove(target);
                }
            }

            //Don't call extra functions if we don't have to.
            if (targetAggroDecay != 0 || selfAggroDecay != 0)
                AddAggro(target, -(targetAggroDecay + selfAggroDecay) * deltaTime);
        }
    }

    public void AttackUpdate()
    {
        //See if there's anything to attack
        attackTarget = FindAttackTarget();

        //Transition to follow or regroup state if we managed to get really far away.
        if (!attackTarget)
        {
            state = (Squad && (Squad.transform.position - transform.position).sqrMagnitude > Squad.range * Squad.range) ?  State.Regroup : State.Follow;
            return;
        }

        //Move to the target
        _charMovement.Goal = attackTarget.transform.position;


        //Find edge-to-edge distance between our colliders
        //Vector3 targetClosestPoint = attackTarget.GetComponent<Collider>().ClosestPoint(transform.position);
        //float pointDistance = Vector3.Distance(transform.position, targetClosestPoint);

        if (Vector3.Distance(transform.position, attackTarget.transform.position) <= meleeAttackDistanceBuffer && (Time.time - attackWait) >= lastAttackTime)
        {
            lastAttackTime = Time.time;
            var director = GetComponent<PlayableDirector>();
            director.time = 0f;
            director.Play();

            attackTarget.GetComponent<NetworkedHealth>().ChangeHealth(-10, gameObject);
        }
    }

    private GameObject FindAttackTarget()
    {
        GameObject target = null;

        if (aggroDict.Count > 0)
        {
            float highestScore = float.NegativeInfinity;
            foreach (var pair in aggroDict)
            {
                if (pair.Value >= aggroProfile.attackThreshold)
                {
                    float newScore = (pair.Value * aggroProfile.GetMultiplier(pair.Key)) - (Vector3.Distance(transform.position, pair.Key.transform.position) * aggroProfile.checkLossPerMeter);
                    if (newScore > highestScore)
                    {
                        target = pair.Key;
                        highestScore = newScore;
                    }
                }
            }
        }

        return target;
    }

    public void FollowUpdate()
    {
        //Check if out of squad range, if so change to Regroup state and return;
        //Transition to Regroup state
        if (Squad && Vector3.Distance(transform.position, Squad.transform.position) > Squad.range)
        {
            state = State.Regroup;
            return;
        }

        //Transition to Attack state if there's /anything/ we're mad enough at.
        if (aggroDict.Values.Any(aggro => aggro >= aggroProfile.attackThreshold))
        {
            state = State.Attack;
            return;
        }

        //Don't set any movement goals here, it's being managed by the Squad.
    }

    public void RegroupUpdate()
    {
        if (Vector3.Distance(transform.position, Squad.transform.position) < Squad.range)
        {
            state = State.Follow;
        }
        else
        {
            _charMovement.Goal = Squad.transform.position;
        }
    }

    private enum ObjectRelation {Self, Friend, Leader, AggroTarget, Other}

    private ObjectRelation FindRelation(GameObject other)
    {
        if (other == gameObject)
            return ObjectRelation.Self;

        if (aggroDict.ContainsKey(gameObject))
            return ObjectRelation.AggroTarget;

        if (!Squad)
            return ObjectRelation.Other;

        if (other == Squad.gameObject)
            return ObjectRelation.Leader;

        if (Squad.minions.Exists(minion => minion.gameObject == other))
            return ObjectRelation.Friend;

        return ObjectRelation.Other;
    }

    private void OnHealthChanged(HealthChangeArgs args)
    {
        //If who hurt who is a mystery, then exit the function.
        if (args.changeSource == null)
            return;

        //first make sure that it was actually damage being done.
        if (args.changeAmount < 0)
        {
            float damage = -args.changeAmount;
            //figure out how to classify our relationship with what just got hurt.
            ObjectRelation woundedRelation = FindRelation(args.senderObject);

            //If it's some random GameObject that's not in our Squad or is not a current target, then we don't care.
            if (woundedRelation == ObjectRelation.Other)
                return;

            ObjectRelation attackerRelation = FindRelation(args.changeSource);

            if (woundedRelation == ObjectRelation.Self)
            {
                //We don't care about self damage or friendly fire.
                if (attackerRelation == ObjectRelation.Self || attackerRelation == ObjectRelation.Leader || attackerRelation == ObjectRelation.Friend)
                    return;

                //Someone just signed themselves up for being attacked!!
                if (attackerRelation == ObjectRelation.Other)
                    AddTarget(args.changeSource);

                AddDamageAggro(aggroProfile.targetAttackedSelf, args.changeSource, damage);
            }
            else if (woundedRelation == ObjectRelation.AggroTarget)
            {
                //If someone is attacking one of our targets (or they're fighting themselves), good on them but we don't care (at the moment when this code was written).
                if (attackerRelation == ObjectRelation.Other || attackerRelation == ObjectRelation.AggroTarget)
                    return;

                //Figure out who hit them and add aggro appropriately.
                if (attackerRelation == ObjectRelation.Self)
                    AddDamageAggro(aggroProfile.selfAttackedTarget, args.senderObject, damage);
                else if (attackerRelation == ObjectRelation.Leader)
                    AddDamageAggro(aggroProfile.leaderAttackedTarget, args.senderObject, damage);
                else if (attackerRelation == ObjectRelation.Friend)
                    AddDamageAggro(aggroProfile.friendAttackedTarget, args.senderObject, damage);

            }
            else if (woundedRelation == ObjectRelation.Leader)
            {
                //We don't care about friendly fire.
                if (attackerRelation == ObjectRelation.Self || attackerRelation == ObjectRelation.Leader || attackerRelation == ObjectRelation.Friend)
                    return;

                if (attackerRelation == ObjectRelation.Other)
                    AddTarget(args.changeSource);

                //Our boss got hurt!
                AddDamageAggro(aggroProfile.targetAttackedLeader, args.changeSource, damage);
            }
            else if (woundedRelation == ObjectRelation.Friend)
            {
                //We don't care about friendly fire.
                if (attackerRelation == ObjectRelation.Self || attackerRelation == ObjectRelation.Leader || attackerRelation == ObjectRelation.Friend)
                    return;

                if (attackerRelation == ObjectRelation.Other)
                    AddTarget(args.changeSource);

                //One of our fellow minions got hurt!
                AddDamageAggro(aggroProfile.targetAttackedFriend, args.changeSource, damage);
            }
        }
    }

    private void AddDamageAggro(float baseAggro, GameObject attacker, float damage)
    {
        aggroDict[attacker] = Mathf.Clamp(aggroDict[attacker] + aggroProfile.CalcDamageAggro(baseAggro, attacker, damage), 0, aggroProfile.maxAggro);
    }

    private void OnDeath(DeathArgs args)
    {
        //Oh, we died.
        if (args.senderObject == gameObject)
            gameObject.SetActive(false);

        //If we're not in a squad, we don't care. Also we can't do this if we don't know who the killer was.
        if (!Squad || args.killer == null)
            return;

        if (aggroDict.ContainsKey(args.senderObject))
        {
            //One of our targets has died, remove it from the list.
            aggroDict.Remove(args.senderObject);
        }
        else
        {
            ObjectRelation killerRelation = FindRelation(args.killer);

            //ignore friendly fire
            if (killerRelation == ObjectRelation.Self || killerRelation == ObjectRelation.Leader || killerRelation == ObjectRelation.Friend)
                return;

            if (killerRelation == ObjectRelation.Other)
                AddTarget(args.killer);

            //One of our friends was just possibly killed by a target!
            if (Squad.gameObject == args.senderObject)
            {
                aggroDict[args.killer] += aggroProfile.CalcAggro(aggroProfile.targetKilledLeader, args.killer);
            }
            else if (Squad.minions.Exists(minion => minion.gameObject == args.senderObject))
            {
                aggroDict[args.killer] += aggroProfile.CalcAggro(aggroProfile.targetKilledFriend, args.killer);
            }
        }
    }
}
