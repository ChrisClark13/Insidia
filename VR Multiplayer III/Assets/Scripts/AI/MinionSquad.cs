using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinionSquad : MonoBehaviour, ISensorListener {
    public int teamNum = 0;

    public List<Minion> minions = new List<Minion>();
    public int capacity = 10;
    public TriggerSensor2 targetSensor;
    public SquadFormation formation;
    public float range = 20f;

    public event Action<MinionSquad, GameObject> TargetAdded;

    private void Start()
    {
        targetSensor.Setup(this, TargetFilter);
        foreach (var minion in minions)
        {
            minion.ChangeSquad(this);
        }
    }

    protected virtual bool TargetFilter(GameObject other)
    {
        //check if it's on another team
        Minion otherMinion = other.GetComponent<Minion>();
        if (otherMinion)
        {
            return (otherMinion.Squad != null) && (otherMinion.Squad.teamNum != this.teamNum);
        }
        else if (other.tag == "Player")
        {
            MinionSquad otherSquad = other.GetComponent<MinionSquad>();
            return (otherSquad && otherSquad.teamNum != this.teamNum);
        }
        return false;
    }

    private void OnEnable()
    {
        StartCoroutine(this.UpdateCoroutine(30f, SquadUpdate));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private List<Minion> GetMinionsInState(Minion.State state)
    {
        return minions.FindAll(minion => minion.state == state);
    }

    private void SquadUpdate()
    {
        //List<GameObject> targetsToRemove = new List<GameObject>(targets.Count);
        //foreach (var target in targets)
        //{
        //    if ((target.transform.position - transform.position).sqrMagnitude > (range * range))
        //        targetsToRemove.Add(target);
        //}
        //targetsToRemove.ForEach(t => targets.Remove(t));

        ////If there's something to attack...
        //if(targetSensor.sensedObjects.Count > 0)
        //{
        //    foreach (var minion in GetMinionsInState(Minion.State.Follow))
        //    {
        //        //... tell the minions to attack!!
        //        minion.state = Minion.State.Attack;
        //    }
        //}
        
        formation.SetMinionGoalsToPositions(transform, GetMinionsInState(Minion.State.Follow));
    }

    public void OnSensorEnter(TriggerSensor2 sensor, GameObject other)
    {
        if (TargetAdded != null)
            TargetAdded(this, other);
    }

    public void OnSensorExit(TriggerSensor2 sensor, GameObject other)
    {

    }
}
