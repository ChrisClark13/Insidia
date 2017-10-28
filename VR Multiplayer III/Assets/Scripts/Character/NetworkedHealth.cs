using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// This script syncs a Health and MaxHealth value back and forth between the server.
/// <para>To use: Place on anything that needs to keep track of it's health. Read the value from <see cref="Health"/> and use <see cref="ChangeHealth(float)"/> or <see cref="SetHealth(float)"/> depending on which one you need.</para>
/// <para>ChangeHealth changes adds that value passed in to the health, SetHealth directly sets the value of the health. Each one also takes a parameter to tell this script what is responsible for the change (be it damage or healing).</para>
/// <para><see cref="SetMaxHealth(float)"/> also exists for setting the <see cref="MaxHealth"/> value.</para>
/// <para>There are instance and static Events for HealthChanged, MaxHealthChanged, and Death that are called automatically when the Health is changed.</para>
/// <para>Created by Christian Clark</para>
/// </summary> 
public class NetworkedHealth : NetworkBehaviour {

    public delegate void ChangeDelegate(HealthChangeArgs args);
    public delegate void DeathDelegate(DeathArgs args);

    [SyncEvent]
    public event ChangeDelegate EventHealthChanged;
    public static event ChangeDelegate HealthChanged;

    [SyncEvent]
    public event ChangeDelegate EventMaxHealthChanged;
    public static event ChangeDelegate MaxHealthChanged;

    [SyncEvent]
    public event DeathDelegate EventDeath;
    public static event DeathDelegate Death;

    [SerializeField]
    [SyncVar]
    private float _health = 100f;
    public float Health { get { return _health; } }

    [SerializeField]
    [SyncVar]
    private float _maxHealth = 100f;
    public float MaxHealth { get { return _maxHealth; } }

    public void ChangeHealth(float changeAmount, GameObject source)
    {
        //If we want to have instant feedback on the client that sent the action (which then gets overwritted/corrected by the server in a bit), this would be the place to do it.

        NetworkIdentity networkIdentity = (source != null) ? source.GetComponentInParent<NetworkIdentity>() : null;
        CmdChangeHealth(changeAmount, ((networkIdentity != null) ? networkIdentity.gameObject : null));
    }

    public void SetHealth(float value, GameObject source)
    {
        NetworkIdentity networkIdentity = (source != null) ? source.GetComponentInParent<NetworkIdentity>() : null;
        CmdSetHealth(value, ((networkIdentity != null) ? networkIdentity.gameObject : null));
    }

    [Command]
    private void CmdChangeHealth(float changeAmount, GameObject source)
    {
        float initalAmount = _health;
        _health = Mathf.Clamp(_health + changeAmount, 0, _maxHealth);
        if (initalAmount != _health)
        {
            OnHealthChanged(source, _health - initalAmount);
        }
    }

    [Command]
    private void CmdSetHealth(float value, GameObject source)
    {
        float initalAmount = _health;
        value = Mathf.Clamp(value, 0, _maxHealth);
        _health = value;
        if (initalAmount != _health)
        {
            OnHealthChanged(source, _health - initalAmount);
        }
    }

    private void OnHealthChanged(GameObject source, float changeAmount)
    {
        EventHealthChanged(new HealthChangeArgs(this, source, _health, changeAmount));
        if (_health == 0)
            EventDeath(new DeathArgs(this, source));
    }

    public void SetMaxHealth(float value, GameObject source)
    {
        NetworkIdentity networkIdentity = (source != null) ? source.GetComponentInParent<NetworkIdentity>() : null;
        CmdSetHealth(value, ((networkIdentity != null) ? networkIdentity.gameObject : null));
    }

    [Command]
    private void CmdSetMaxHealth(float value, GameObject source)
    {
        float initalAmount = _maxHealth;
        //Make sure Max Health is not less than zero.
        value = Mathf.Max(value, 0);
        _maxHealth = value;
        if (initalAmount != _health)
        {
            OnMaxHealthChanged(source, initalAmount - _maxHealth);
        }
    }

    private void OnMaxHealthChanged(GameObject source, float changeAmount)
    {
        EventHealthChanged(new HealthChangeArgs(this, source, _health, changeAmount));

        //If the Max Health is now lower than the Health, set Health to Max Health to cap that value.
        if (_health > _maxHealth)
            CmdSetHealth(_maxHealth, source);
    }

    private void CallStaticHealthChanged(HealthChangeArgs args)
    {
        if (HealthChanged != null)
            HealthChanged(args);
    }

    private void CallStaticMaxHealthChanged(HealthChangeArgs args)
    {
        if (MaxHealthChanged != null)
            MaxHealthChanged(args);
    }

    private void CallStaticDeath(DeathArgs args)
    {
        if (Death != null)
            Death(args);
    }

    private void OnEnable()
    {
        EventHealthChanged += CallStaticHealthChanged;
        EventMaxHealthChanged += CallStaticMaxHealthChanged;
        EventDeath += CallStaticDeath;
    }

    private void Start()
    {
        _health = _maxHealth;
    }

    private void OnDisable()
    {
        EventHealthChanged -= CallStaticHealthChanged;
        EventMaxHealthChanged -= CallStaticMaxHealthChanged;
        EventDeath -= CallStaticDeath;
    }

    // For correcting the values when they get set in the editor.
    private void OnValidate()
    {
        _maxHealth = Mathf.Max(_maxHealth, 0);
        _health = Mathf.Clamp(_health, 0, _maxHealth);

        //If we mess with this in the editor while we're playing, send the change out to everyone!
        if (Application.isPlaying && isServer)
        {
            OnHealthChanged(null, 0f);
            OnMaxHealthChanged(null, 0f);
        }
    }
}

//structs holding data about the HealthChange and Death events. This way if there needs to be a change to them, it won't break existing code (as much).
//These can be sent (more or less) safely over a network.

/// <summary>
/// Holds GameObject <see cref="senderObject"/>, NetworkedHealth <see cref="sender"/>, float <see cref="newValue"/>, and float <see cref="changeAmount"/>.
/// </summary>
[Serializable]
public struct HealthChangeArgs
{
    public GameObject senderObject;
    private NetworkedHealth _sender;
    public NetworkedHealth sender {
        get
        {
            //When this struct is sent over the network, the _sender value is not sent with it and so becomes null and in need of being reset to the correct value.
            //This is because specific references to scripts on GameObjects cannot be sent over a network
            //(or actually, GameObject references for that matter, but Unity sends an ID and handles getting the matching reference on the other computer 
            //for us as long as the GameObject has a NetworkIdentity attached to it).
            if (_sender == null)
                _sender = senderObject.GetComponent<NetworkedHealth>();
            return _sender;
        }
    }
    public GameObject changeSource;
    public float newValue;
    public float changeAmount;

    public HealthChangeArgs(NetworkedHealth sender, GameObject changeSource, float newValue, float changeAmount)
    {
        senderObject = sender.gameObject;
        _sender = sender;
        this.changeSource = changeSource;
        this.newValue = newValue;
        this.changeAmount = changeAmount;
    }
}

/// <summary>
/// Holds GameObject <see cref="senderObject"/>, NetworkedHealth <see cref="sender"/>, and GameObject <see cref="killer"/>.
/// </summary>
[Serializable]
public struct DeathArgs
{
    public GameObject senderObject;
    private NetworkedHealth _sender;
    public NetworkedHealth sender
    {
        get
        {
            if (_sender == null)
                _sender = senderObject.GetComponent<NetworkedHealth>();
            return _sender;
        }
    }
    public GameObject killer;

    public DeathArgs(NetworkedHealth sender, GameObject killer)
    {
        senderObject = sender.gameObject;
        _sender = sender;
        this.killer = killer;
    }
}